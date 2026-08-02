output "docdb_endpoint" {
  value = aws_docdb_cluster.catalog.endpoint
}

output "redis_endpoint" {
  value = aws_elasticache_replication_group.redis.primary_endpoint_address
}

output "ordering_db_endpoint" {
  value = aws_db_instance.ordering.address
}

output "inventory_db_endpoint" {
  value = aws_db_instance.inventory.address
}