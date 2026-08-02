module "eks" {
  source          = "terraform-aws-modules/eks/aws"
  version         = "~> 20.0"

  cluster_name    = "${var.project}-${var.env}-eks"
  cluster_version = "1.31"

  vpc_id                          = var.vpc_id
  subnet_ids                      = var.private_subnets
  cluster_endpoint_public_access  = true

  cluster_addons = {
    coredns   = {most_recent = true}
    kube-proxy = {most_recent = true}
    vpc-cni   = {most_recent = true}
  }

  eks_managed_node_groups = {
    system  = {
      name          = "system"
      instance_types = ["t3.medium"]
      min_size      = 2
      max_size      = 2
      desired_size  = 2

      labels = {role = "system"}
    }

    app = {
      name          = "app"
      instance_types = ["t3.large"]
      min_size      = 2
      max_size      = 10
      desired_size  = 2

      labels = {role = "app"}
    }
  }

  tags = var.tags
}