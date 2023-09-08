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

variable "pipeline_component_service_plans" {
  type = object({
    coordinator_service_plan_sku     = string
    coordinator_minimum_instances    = number
    coordinator_maximum_instances    = number
    pdf_generator_service_plan_sku   = string
    pdf_generator_minimum_instances  = number
    pdf_generator_maximum_instances  = number
    text_extractor_plan_sku          = string
    text_extractor_minimum_instances = number
    text_extractor_maximum_instances = number
  })
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

variable "overnight_clear_down_enabled" {
  type = bool
}

variable "pipeline_event_hub_settings" {
  type = object({
    sku      = string
    capacity = number
  })
}

variable "clear_down_enabled" {
  type = bool
}

variable "clear_down_input_days" {
  type = number
}