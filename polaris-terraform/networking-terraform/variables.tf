
#################### Variables ####################

variable "resource_name_prefix" {
  default = "networking"
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

variable "polarisPipelineTextExtractorSubnet" {
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

variable "polarisAuthHandoverSubnet" {
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