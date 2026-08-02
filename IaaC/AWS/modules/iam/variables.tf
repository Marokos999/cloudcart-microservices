variable "project" {
  type = string
}

variable "env" {
  type = string
}

variable "cluster_name" {
  type = string
}

variable "eks_oidc_issuer_url" {
  type = string
}

variable "tags" {
  type    = map(string)
  default = {}
}