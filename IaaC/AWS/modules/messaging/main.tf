resource "aws_mq_broker" "rabbitmq" {
  broker_name        = "${var.project}-${var.env}-rabbitmq"
  engine_type        = "RabbitMQ"
  engine_version     = "3.13"
  host_instance_type = var.instance_type
  deployment_mode    = var.env == "prod" ? "CLUSTER_MULTI_AZ" : "SINGLE_INSTANCE"

  subnet_ids         = var.env == "prod" ? var.private_subnets : [var.private_subnets[0]]
  security_groups    = [var.security_group_id]

  publicly_accessible = false

  user {
    username = var.rabbitmq_username
    password = var.rabbitmq_password
  }

  logs {
    general = true
  }

  tags = var.tags
}