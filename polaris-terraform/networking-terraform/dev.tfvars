#touched file to prompt a PR build

environment = {
  name  = "development"
  alias = "dev"
}

ddeiServicesSubnet                  = "10.7.196.64/27"
polarisPipelineSaSubnet             = "10.7.196.96/28"
polarisPipelineCoordinatorSubnet    = "10.7.196.112/28"
polarisPipelinePdfGeneratorSubnet   = "10.7.196.128/28"
polarisPipelinePdfRedactorSubnet    = "10.7.197.224/28"
polarisPipelineTextExtractorSubnet  = "10.7.196.144/28"
polarisAlertNotificationsSubnet     = "10.7.197.128/28"
polarisPipelineTextExtractor2Subnet = "10.7.197.0/28"
polarisGatewaySubnet                = "10.7.196.176/28"
polarisUiSubnet                     = "10.7.196.0/28"
polarisProxySubnet                  = "10.7.196.160/28"
polarisAppsSubnet                   = "10.7.196.224/27"
polarisCiSubnet                     = "10.7.196.48/28"
mockCmsServiceSubnet                = "10.7.197.48/28"
polarisAmplsSubnet                  = "10.7.197.96/27"
polarisPipelineSa2Subnet            = "10.7.197.160/27"
polarisScaleSetSubnet               = "10.7.197.192/27"
polarisApps2Subnet                  = "10.7.196.192/27"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Development"

insights_configuration = {
  log_retention_days                   = 90
  log_total_retention_days             = 2555
  analytics_internet_ingestion_enabled = false
  analytics_internet_query_enabled     = false
  insights_internet_ingestion_enabled  = true
  insights_internet_query_enabled      = false
}