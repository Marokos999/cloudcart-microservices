# ── DocumentDB (MongoDB compatible) — Catalog ──────────────────────────────
resource "aws_docdb_subnet_group" "main" {
  name       = "${var.project}-${var.env}-docdb"
  subnet_ids = var.private_subnets
  tags       = var.tags
}

resource "aws_docdb_cluster" "catalog" {
  cluster_identifier      = "${var.project}-${var.env}-catalog"
  engine                  = "docdb"
  master_username         = "catalogadmin"
  master_password         = var.docdb_password
  db_subnet_group_name    = aws_docdb_subnet_group.main.name
  vpc_security_group_ids  = [var.db_security_group_id]
  skip_final_snapshot     = var.env != "prod"
  tags                    = var.tags
}

resource "aws_docdb_cluster_instance" "catalog" {
  count              = var.env == "prod" ? 2 : 1
  identifier         = "${var.project}-${var.env}-catalog-${count.index}"
  cluster_identifier = aws_docdb_cluster.catalog.id
  instance_class     = var.docdb_instance_class
  tags               = var.tags
}

# ── ElastiCache Redis — Basket + Inventory ──────────────────────────────────
resource "aws_elasticache_subnet_group" "main" {
  name       = "${var.project}-${var.env}-redis"
  subnet_ids = var.private_subnets
  tags       = var.tags
}

resource "aws_elasticache_replication_group" "redis" {
  replication_group_id = "${var.project}-${var.env}-redis"
  description          = "CloudCart Redis"
  node_type            = var.redis_node_type
  num_cache_clusters   = var.env == "prod" ? 2 : 1
  port                 = 6379
  subnet_group_name    = aws_elasticache_subnet_group.main.name
  security_group_ids   = [var.db_security_group_id]
  at_rest_encryption_enabled = true
  transit_encryption_enabled = true
  tags                 = var.tags
}

# ── RDS SQL Server — Ordering ───────────────────────────────────────────────
resource "aws_db_instance" "ordering" {
  identifier             = "${var.project}-${var.env}-ordering"
  engine                 = "sqlserver-ex"
  engine_version         = "15.00"
  instance_class         = var.rds_instance_class
  username               = "orderingadmin"
  password               = var.ordering_db_password
  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [var.db_security_group_id]
  skip_final_snapshot    = var.env != "prod"
  allocated_storage      = 20
  license_model          = "license-included"
  tags                   = var.tags
}

# ── RDS SQL Server — Inventory ──────────────────────────────────────────────
resource "aws_db_instance" "inventory" {
  identifier             = "${var.project}-${var.env}-inventory"
  engine                 = "sqlserver-ex"
  engine_version         = "15.00"
  instance_class         = var.rds_instance_class
  username               = "inventoryadmin"
  password               = var.inventory_db_password
  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [var.db_security_group_id]
  skip_final_snapshot    = var.env != "prod"
  allocated_storage      = 20
  license_model          = "license-included"
  tags                   = var.tags
}

resource "aws_db_subnet_group" "main" {
  name       = "${var.project}-${var.env}-rds"
  subnet_ids = var.private_subnets
  tags       = var.tags
}