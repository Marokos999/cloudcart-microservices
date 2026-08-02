output "prometheus_endpoint" {
  value = aws_prometheus_workspace.main.prometheus_endpoint
}

output "grafana_endpoint" {
  value = aws_grafana_workspace.main.endpoint
}

output "log_group_name" {
  value = aws_cloudwatch_log_group.eks.name
}