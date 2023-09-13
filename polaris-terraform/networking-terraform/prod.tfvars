environment = {
  name  = "production"
  alias = "prod"
}

vnetAddressSpace = "10.7.204.0/23"
vnetDnsServer    = "10.7.204.164"

cmsServicesSubnet                  = "10.7.204.0/28"
ddeiServicesSubnet                 = "10.7.204.32/27"
polarisPipelineSaSubnet            = "10.7.204.96/28"
polarisPipelineCoordinatorSubnet   = "10.7.204.208/28"
polarisPipelinePdfGeneratorSubnet  = "10.7.204.80/28"
polarisPipelineTextExtractorSubnet = "10.7.205.0/28"
polarisPipelineKeyVaultSubnet      = "10.7.204.64/29"
polarisGatewaySubnet               = "10.7.204.192/28"
polarisUiSubnet                    = "10.7.204.16/28"
polarisProxySubnet                 = "10.7.204.112/28"
polarisAppsSubnet                  = "10.7.204.224/27"
polarisCiSubnet                    = "10.7.204.176/28"
polarisDnsResolveSubnet            = "10.7.204.160/28"
gatewaySubnet                      = "10.7.204.128/27"
polarisAuthHandoverSubnet          = "10.7.205.16/28"
mockCmsServiceSubnet               = "10.7.205.48/28"
polarisAmplsSubnet                 = "10.7.205.64/27"
polarisPipelineNetheriteSubnet     = "10.7.205.96/27"
polarisPipelineSa2Subnet           = "10.7.205.128/27"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"