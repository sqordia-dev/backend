# Route 53 for Custom Domain

# Route 53 Hosted Zone (if domain is provided)
resource "aws_route53_zone" "api" {
  count = var.domain_name != "" ? 1 : 0
  name  = var.domain_name

  tags = {
    Name        = "${var.project_name}-route53-${var.environment}"
    Environment = var.environment
    Project     = var.project_name
  }
}

# ACM Certificate for HTTPS
resource "aws_acm_certificate" "api" {
  count            = var.domain_name != "" ? 1 : 0
  domain_name      = var.domain_name
  validation_method = "DNS"

  lifecycle {
    create_before_destroy = true
  }

  tags = {
    Name        = "${var.project_name}-cert-${var.environment}"
    Environment = var.environment
    Project     = var.project_name
  }
}

# Route 53 Record for Certificate Validation
resource "aws_route53_record" "cert_validation" {
  for_each = var.domain_name != "" ? {
    for dvo in aws_acm_certificate.api[0].domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  } : {}

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = aws_route53_zone.api[0].zone_id
}

# ACM Certificate Validation
# Note: This will wait for DNS propagation. The HTTP listener will work immediately.
resource "aws_acm_certificate_validation" "api" {
  count           = var.domain_name != "" ? 1 : 0
  certificate_arn = aws_acm_certificate.api[0].arn
  validation_record_fqdns = [for record in aws_route53_record.cert_validation : record.fqdn]
  
  # Increase timeout for DNS propagation
  timeouts {
    create = "2h"
  }
}

# Route 53 A Record pointing to ALB
resource "aws_route53_record" "api" {
  count   = var.domain_name != "" ? 1 : 0
  zone_id = aws_route53_zone.api[0].zone_id
  name    = var.domain_name
  type    = "A"

  alias {
    name                   = aws_lb.api[0].dns_name
    zone_id                = aws_lb.api[0].zone_id
    evaluate_target_health = true
  }
}

