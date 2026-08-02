output "eks_cluster_name" {
  value = module.eks.cluster_name
}

output "eks_cluster_endpoint" {
  value = module.eks.cluster_endpoint
}

output "ecr_repository_urls" {
  value = module.ecr.repository_urls
}

output "cloudfront_domain" {
  value = module.frontend.cloudfront_domain
}

output "prometheus_endpoint" {
  value = module.monitoring.prometheus_endpoint
}

output "grafana_endpoint" {
  value = module.monitoring.grafana_endpoint
}

output "rabbitmq_endpoint" {
  value = module.messaging.rabbitmq_endpoint
}

output "docdb_endpoint" {
  value     = module.databases.docdb_endpoint
  sensitive = true
}

output "redis_endpoint" {
  value     = module.databases.redis_endpoint
  sensitive = true
}