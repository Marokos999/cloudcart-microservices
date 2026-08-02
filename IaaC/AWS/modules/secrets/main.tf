resource "aws_secretsmanager_secret" "secrets" {
  for_each = var.secrets

  name                    = "cloudcart/${var.env}/${each.key}"
  recovery_window_in_days = var.env == "prod" ? 30 : 0

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "secrets" {
  for_each = var.secrets

  secret_id     = aws_secretsmanager_secret.secrets[each.key].id
  secret_string = each.value
}