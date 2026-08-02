terraform {
  backend "s3" {
    bucket         = "cloudcart-terraform-state"
    key            = "staging/terraform.tfstate"
    region         = "eu-central-1"
    dynamodb_table = "cloudcart-terraform-locks"
    encrypt        = true
  }
}