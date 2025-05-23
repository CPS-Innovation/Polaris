environment = {
  name  = "uat"
  alias = "uat"
}

ddeiServicesSubnet                         = "10.7.200.128/27"
polarisPipelineSaSubnet                    = "10.7.200.96/28"
polarisPipelineCoordinatorSubnet           = "10.7.200.208/28"
polarisPipelinePdfGeneratorSubnet          = "10.7.200.80/28"
polarisPipelinePdfThumbnailGeneratorSubnet = "10.7.201.32/28"
polarisPipelinePdfRedactorSubnet           = "10.7.201.96/28"
polarisAlertNotificationsSubnet            = "10.7.201.0/28"
polarisPipelineTextExtractor2Subnet        = "10.7.200.16/28"
polarisGatewaySubnet                       = "10.7.200.192/28"
polarisUiSubnet                            = "10.7.200.0/28"
polarisProxySubnet                         = "10.7.200.112/28"
polarisAppsSubnet                          = "10.7.200.224/27"
polarisCiSubnet                            = "10.7.200.176/28"
mockCmsServiceSubnet                       = "10.7.201.48/28"
polarisAmplsSubnet                         = "10.7.201.64/27"
polarisPipelineSa2Subnet                   = "10.7.201.128/27"
polarisScaleSetSubnet                      = "10.7.201.160/27"
polarisApps2Subnet                         = "10.7.201.192/27"
polarisMaintenanceSubnet                   = "10.7.201.240/29"
polarisDnsResolverInboundSubnet            = "10.7.200.64/28"
polarisDnsResolverOutboundSubnet           = "10.7.200.160/28"

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
dns_server    = "10.7.200.68"