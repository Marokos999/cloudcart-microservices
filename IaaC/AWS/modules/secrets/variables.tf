variable "env" {
  type = string
}

variable "secrets" {
  type = map(string)
}

variable "tags" {
  type    = map(string)
  default = {}
}