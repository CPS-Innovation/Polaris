
#################### Variables ####################

variable "resource_name_prefix" {
  default = "networking"
}

variable "app_name_prefix" {
  default = "polaris"
}

variable "environment" {
  type = object({
    name  = string
    alias = string
  })
}

variable "location" {
  default = "UK South"
}

variable "vnetAddressSpace" {
}

variable "ddeiServicesSubnet" {
}

variable "polarisPipelineSaSubnet" {
}

variable "polarisPipelineCoordinatorSubnet" {
}

variable "polarisPipelinePdfGeneratorSubnet" {
}

variable "polarisPipelinePdfRedactorSubnet" {
}

variable "polarisPipelineTextExtractorSubnet" {
}

variable "polarisPipelineTextExtractor2Subnet" {
}

variable "polarisGatewaySubnet" {
}

variable "polarisUiSubnet" {
}

variable "polarisProxySubnet" {
}

variable "polarisAppsSubnet" {
}

variable "polarisCiSubnet" {
}

variable "polarisDnsResolveSubnet" {
}

variable "gatewaySubnet" {
}

variable "mockCmsServiceSubnet" {
}

variable "polarisAmplsSubnet" {
}

variable "polarisPipelineSa2Subnet" {
}

variable "polarisScaleSetSubnet" {
}

variable "polarisApps2Subnet" {
}

variable "terraform_service_principal_display_name" {
  type = string
}

variable "vnetDnsServer" {
  type = string
}

variable "insights_log_retention_days" {
  type = number
}

variable "insights_log_total_retention_days" {
  type = number
}