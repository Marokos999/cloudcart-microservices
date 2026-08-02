variable "project" { type = string }
variable "env"     { type = string }

variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}

variable "private_subnets" { type = list(string) }
variable "public_subnets"  { type = list(string) }

variable "tags" {
  type    = map(string)
  default = {}
}