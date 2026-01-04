# Application Load Balancer for Custom Domain

# ALB Security Group (only created if domain is configured)
resource "aws_security_group" "alb" {
  count       = var.domain_name != "" ? 1 : 0
  name        = "${var.project_name}-alb-${var.environment}"
  description = "Security group for Application Load Balancer"
  vpc_id      = aws_vpc.main.id

  # Allow HTTP from internet
  ingress {
    description = "Allow HTTP from internet"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Allow HTTPS from internet
  ingress {
    description = "Allow HTTPS from internet"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Allow outbound to ECS (using CIDR blocks to avoid circular dependency)
  egress {
    description = "Allow outbound to ECS"
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = [aws_vpc.main.cidr_block]
  }

  tags = {
    Name        = "${var.project_name}-alb-sg-${var.environment}"
    Environment = var.environment
    Project     = var.project_name
  }
}

# Application Load Balancer (only created if domain is configured)
resource "aws_lb" "api" {
  count              = var.domain_name != "" ? 1 : 0
  name               = "${var.project_name}-api-${var.environment}"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb[0].id]
  subnets            = [aws_subnet.public_1.id, aws_subnet.public_2.id]

  enable_deletion_protection = false

  tags = {
    Name        = "${var.project_name}-alb-${var.environment}"
    Environment = var.environment
    Project     = var.project_name
  }
}

# ALB Target Group
resource "aws_lb_target_group" "api" {
  count       = var.domain_name != "" ? 1 : 0
  name        = "${var.project_name}-api-${var.environment}"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    path                = "/api/health"
    protocol            = "HTTP"
    matcher             = "200"
  }

  deregistration_delay = 30

  tags = {
    Name        = "${var.project_name}-api-tg-${var.environment}"
    Environment = var.environment
    Project     = var.project_name
  }
}

# ALB HTTP Listener
# Temporarily forwards to target group until HTTPS is ready, then redirects to HTTPS
resource "aws_lb_listener" "api_http" {
  count             = var.domain_name != "" ? 1 : 0
  load_balancer_arn = aws_lb.api[0].arn
  port              = "80"
  protocol          = "HTTP"

  # Forward to target group initially (allows ECS service to connect)
  # Once HTTPS listener is created, this will be updated to redirect
  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api[0].arn
  }
}

# ALB HTTPS Listener
# This will be created after certificate validation completes (after DNS is configured)
resource "aws_lb_listener" "api_https" {
  count             = var.domain_name != "" ? 1 : 0
  load_balancer_arn = aws_lb.api[0].arn
  port              = "443"
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = aws_acm_certificate_validation.api[0].certificate_arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api[0].arn
  }
}

# Note: ECS Service is updated in ecs.tf to include load_balancer configuration
# The HTTPS listener must be created before the ECS service can use the target group

