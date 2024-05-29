
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

variable "polarisAlertNotificationsSubnet" {
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

variable "insights_configuration" {
  type = object({
    log_retention_days                   = number
    log_total_retention_days             = number
    analytics_internet_ingestion_enabled = bool
    analytics_internet_query_enabled     = bool
    insights_internet_ingestion_enabled  = bool
    insights_internet_query_enabled      = bool
  })
}