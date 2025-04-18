environment = {
  name  = "qa"
  alias = "qa"
}

ddeiServicesSubnet                         = "10.7.198.32/27"
polarisPipelineSaSubnet                    = "10.7.198.96/28"
polarisPipelineCoordinatorSubnet           = "10.7.198.208/28"
polarisPipelinePdfGeneratorSubnet          = "10.7.198.80/28"
polarisPipelinePdfThumbnailGeneratorSubnet = "10.7.199.112/28"
polarisPipelinePdfRedactorSubnet           = "10.7.199.96/28"
polarisAlertNotificationsSubnet            = "10.7.199.0/28"
polarisPipelineTextExtractor2Subnet        = "10.7.198.16/28"
polarisGatewaySubnet                       = "10.7.198.192/28"
polarisUiSubnet                            = "10.7.198.0/28"
polarisProxySubnet                         = "10.7.198.112/28"
polarisAppsSubnet                          = "10.7.198.224/27"
polarisCiSubnet                            = "10.7.198.176/28"
mockCmsServiceSubnet                       = "10.7.199.48/28"
polarisAmplsSubnet                         = "10.7.199.64/27"
polarisPipelineSa2Subnet                   = "10.7.199.128/27"
polarisScaleSetSubnet                      = "10.7.199.160/27"
polarisApps2Subnet                         = "10.7.199.192/27"
polarisMaintenanceSubnet                   = "10.7.199.240/29"
polarisDnsResolverInboundSubnet            = "10.7.198.160/28"
polarisDnsResolverOutboundSubnet           = "10.7.198.64/28"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-QA"

insights_configuration = {
  log_retention_days                   = 90
  log_total_retention_days             = 2555
  analytics_internet_ingestion_enabled = false
  analytics_internet_query_enabled     = false
  insights_internet_ingestion_enabled  = false
  insights_internet_query_enabled      = false
}

teams_account = "Mark.Jones3@cps.gov.uk"
dns_server    = "10.7.198.164"