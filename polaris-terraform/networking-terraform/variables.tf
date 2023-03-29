
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

variable "cmsServicesSubnet" {
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

variable "polarisPipelineKeyVaultSubnet" {
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

variable "terraform_service_principal_display_name" {
  type = string
}

variable "nsg_name" {
}