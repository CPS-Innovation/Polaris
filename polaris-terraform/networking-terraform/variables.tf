
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

############## VPN-related variables ##############

variable "create_vpn" {
  description = "VPN - Whether to create the VPN"
  type        = bool
  default     = false
}

variable "vpn_client_ip_pool" {
  description = "VPN - CIDR ranges from which VPN will assign IPs to clients upon connection"
  type        = list(string)
  default     = ["10.1.0.0/24"]
}

variable "vpn_aad_audience_id" {
  description = "VPN - Azure AD Application (client) ID, for granting VPN access from. Passed into the audience argument of the VPN configuration"
  type        = string
  default     = ""
}