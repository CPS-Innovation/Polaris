environment = {
  name  = "production"
  alias = "prod"
}

ddeiServicesSubnet                         = "10.7.204.32/27"
polarisPipelineSaSubnet                    = "10.7.204.96/28"
polarisPipelineCoordinatorSubnet           = "10.7.204.208/28"
polarisPipelinePdfGeneratorSubnet          = "10.7.204.80/28"
polarisPipelinePdfThumbnailGeneratorSubnet = "10.7.205.96/28"
polarisPipelinePdfRedactorSubnet           = "10.7.205.224/28"
polarisAlertNotificationsSubnet            = "10.7.205.0/28"
polarisPipelineTextExtractor2Subnet        = "10.7.204.0/28"
polarisGatewaySubnet                       = "10.7.204.192/28"
polarisUiSubnet                            = "10.7.204.16/28"
polarisProxySubnet                         = "10.7.204.112/28"
polarisAppsSubnet                          = "10.7.204.224/27"
polarisCiSubnet                            = "10.7.204.176/28"
mockCmsServiceSubnet                       = "10.7.205.48/28"
polarisAmplsSubnet                         = "10.7.205.64/27"
polarisPipelineSa2Subnet                   = "10.7.205.128/27"
polarisScaleSetSubnet                      = "10.7.205.160/27"
polarisApps2Subnet                         = "10.7.205.192/27"
polarisMaintenanceSubnet                   = "10.7.205.240/29"
polarisDnsResolverInboundSubnet            = "10.7.204.160/28"
polarisDnsResolverOutboundSubnet           = "10.7.204.64/28"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"

insights_configuration = {
  log_retention_days                   = 90
  log_total_retention_days             = 2555
  analytics_internet_ingestion_enabled = false
  analytics_internet_query_enabled     = false
  insights_internet_ingestion_enabled  = false
  insights_internet_query_enabled      = false
}

teams_account = "Mark.Jones3@cps.gov.uk"
dns_server    = "10.7.204.164"