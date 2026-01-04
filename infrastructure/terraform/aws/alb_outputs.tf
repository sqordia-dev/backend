# ALB and Domain Outputs

output "alb_dns_name" {
  description = "DNS name of the Application Load Balancer"
  value       = var.domain_name != "" ? aws_lb.api[0].dns_name : null
}

output "alb_arn" {
  description = "ARN of the Application Load Balancer"
  value       = var.domain_name != "" ? aws_lb.api[0].arn : null
}

output "api_url" {
  description = "API base URL"
  value       = var.domain_name != "" ? "https://${var.domain_name}" : "http://<task-public-ip>:8080"
}

output "api_url_http" {
  description = "API base URL (HTTP, redirects to HTTPS if domain configured)"
  value       = var.domain_name != "" ? "http://${var.domain_name}" : "http://<task-public-ip>:8080"
}

output "route53_zone_id" {
  description = "Route 53 hosted zone ID"
  value       = var.domain_name != "" ? aws_route53_zone.api[0].zone_id : null
}

output "route53_name_servers" {
  description = "Route 53 name servers (update your domain registrar with these)"
  value       = var.domain_name != "" ? aws_route53_zone.api[0].name_servers : null
}

output "acm_certificate_arn" {
  description = "ACM certificate ARN for HTTPS"
  value       = var.domain_name != "" ? aws_acm_certificate.api[0].arn : null
}

