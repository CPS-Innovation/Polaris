#touched file to prompt a PR build

environment = {
  name  = "development"
  alias = "dev"
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Development"

insights_configuration = {
  log_retention_days                   = 90
  log_total_retention_days             = 2555
  analytics_internet_ingestion_enabled = false
  analytics_internet_query_enabled     = false
  insights_internet_ingestion_enabled  = true
  insights_internet_query_enabled      = false
}