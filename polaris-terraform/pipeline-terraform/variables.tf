#################### Variables ####################

variable "resource_name_prefix" {
  type    = string
  default = "polaris-pipeline"
}

variable "polaris_resource_name_prefix" {
  type    = string
  default = "polaris"
}

variable "ddei_resource_name_prefix" {
  type    = string
  default = "polaris-ddei"
}

variable "env" {
  type = string
}

variable "coordinator_service_plan_sku" {
  type = string
}

variable "app_service_plan_sku" {
  type = string
}

variable "environment_tag" {
  type        = string
  description = "Environment tag value"
}

variable "networking_resource_name_suffix" {
  default = "networking"
}

variable "dns_server" {
  type = string
}

variable "terraform_service_principal_display_name" {
  type = string
}

variable "pipeline_logging" {
  type = object({
    coordinator_scale_controller    = string
    pdf_generator_scale_controller  = string
    text_extractor_scale_controller = string
  })
}