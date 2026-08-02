output "lb_controller_role_arn" {
  value = module.lb_controller_role.iam_role_arn
}

output "external_secrets_role_arn" {
  value = module.external_secrets_role.iam_role_arn
}

output "cluster_autoscaler_role_arn" {
  value = module.cluster_autoscaler_role.iam_role_arn
}

output "oidc_provider_arn" {
  value = aws_iam_openid_connect_provider.eks.arn
}