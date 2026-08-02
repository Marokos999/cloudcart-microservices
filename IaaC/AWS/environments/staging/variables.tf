variable "aws_region" {
  type    = string
  default = "eu-central-1"
}

variable "project" {
  type    = string
  default = "cloudcart"
}

variable "env" {
  type    = string
  default = "staging"
}

variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}

variable "private_subnets" {
  type    = list(string)
  default = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
}

variable "public_subnets" {
  type    = list(string)
  default = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]
}

variable "db_security_group_id" {
  type = string
}

variable "docdb_password" {
  type      = string
  sensitive = true
}

variable "ordering_db_password" {
  type      = string
  sensitive = true
}

variable "inventory_db_password" {
  type      = string
  sensitive = true
}

variable "rabbitmq_username" {
  type    = string
  default = "cloudcart"
}

variable "rabbitmq_password" {
  type      = string
  sensitive = true
}

variable "secrets" {
  type    = map(string)
  default = {}
}