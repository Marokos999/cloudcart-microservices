# ── CloudWatch Log Group ────────────────────────────────────────────────────
resource "aws_cloudwatch_log_group" "eks" {
  name              = "/aws/eks/${var.project}-${var.env}/cluster"
  retention_in_days = var.env == "prod" ? 90 : 14
  tags              = var.tags
}

# ── Amazon Managed Prometheus ───────────────────────────────────────────────
resource "aws_prometheus_workspace" "main" {
  alias = "${var.project}-${var.env}"
  tags  = var.tags
}

# ── Amazon Managed Grafana ──────────────────────────────────────────────────
resource "aws_grafana_workspace" "main" {
  name                     = "${var.project}-${var.env}-grafana"
  account_access_type      = "CURRENT_ACCOUNT"
  authentication_providers = ["AWS_SSO"]
  permission_type          = "SERVICE_MANAGED"
  data_sources             = ["PROMETHEUS", "CLOUDWATCH"]
  role_arn                 = aws_iam_role.grafana.arn
  tags                     = var.tags
}

resource "aws_iam_role" "grafana" {
  name = "${var.project}-${var.env}-grafana"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = { Service = "grafana.amazonaws.com" }
      Action    = "sts:AssumeRole"
    }]
  })

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "grafana_cloudwatch" {
  role       = aws_iam_role.grafana.name
  policy_arn = "arn:aws:iam::aws:policy/CloudWatchReadOnlyAccess"
}