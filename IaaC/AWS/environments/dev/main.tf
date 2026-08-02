provider "aws" {
  region = var.aws_region

  default_tags {
    tags = local.tags
  }
}

locals {
  tags = {
    project     = var.project
    env         = var.env
    managed_by  = "terraform"
  }
}

module "networking" {
  source          = "../../modules/networking"
  project         = var.project
  env             = var.env
  vpc_cidr        = var.vpc_cidr
  private_subnets = var.private_subnets
  public_subnets  = var.public_subnets
  tags            = local.tags
}

module "eks" {
  source          = "../../modules/eks"
  project         = var.project
  env             = var.env
  vpc_id          = module.networking.vpc_id
  private_subnets = module.networking.private_subnets
  tags            = local.tags
}

module "iam" {
  source              = "../../modules/iam"
  project             = var.project
  env                 = var.env
  cluster_name        = module.eks.cluster_name
  eks_oidc_issuer_url = module.eks.oidc_issuer_url
  tags                = local.tags
}

module "ecr" {
  source = "../../modules/ecr"
  tags   = local.tags
}

module "secrets" {
  source  = "../../modules/secrets"
  env     = var.env
  secrets = var.secrets
  tags    = local.tags
}

module "databases" {
  source                = "../../modules/databases"
  project               = var.project
  env                   = var.env
  private_subnets       = module.networking.private_subnets
  db_security_group_id  = var.db_security_group_id
  docdb_password        = var.docdb_password
  ordering_db_password  = var.ordering_db_password
  inventory_db_password = var.inventory_db_password
  tags                  = local.tags
}

module "messaging" {
  source            = "../../modules/messaging"
  project           = var.project
  env               = var.env
  private_subnets   = module.networking.private_subnets
  security_group_id = var.db_security_group_id
  rabbitmq_username = var.rabbitmq_username
  rabbitmq_password = var.rabbitmq_password
  tags              = local.tags
}

module "monitoring" {
  source  = "../../modules/monitoring"
  project = var.project
  env     = var.env
  tags    = local.tags
}

module "frontend" {
  source  = "../../modules/frontend"
  project = var.project
  env     = var.env
  tags    = local.tags
}