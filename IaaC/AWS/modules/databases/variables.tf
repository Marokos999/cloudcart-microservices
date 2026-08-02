variable "project" {
  type = string
}

variable "env" {
  type = string
}

variable "private_subnets" {
  type = list(string)
}

variable "db_security_group_id" {
  type = string
}

variable "docdb_password" {
  type      = string
  sensitive = true
}

variable "docdb_instance_class" {
  type    = string
  default = "db.t3.medium"
}

variable "redis_node_type" {
  type    = string
  default = "cache.t3.micro"
}

variable "ordering_db_password" {
  type      = string
  sensitive = true
}

variable "inventory_db_password" {
  type      = string
  sensitive = true
}

variable "rds_instance_class" {
  type    = string
  default = "db.t3.small"
}

variable "tags" {
  type    = map(string)
  default = {}
}