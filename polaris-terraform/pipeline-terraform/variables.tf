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
    coordinator_service_plan_sku           = string
    coordinator_always_ready_instances     = number
    coordinator_maximum_scale_out_limit    = number
    coordinator_plan_maximum_burst         = number
    pdf_generator_service_plan_sku         = string
    pdf_generator_always_ready_instances   = number
    pdf_generator_maximum_scale_out_limit  = number
    pdf_generator_plan_maximum_burst       = number
    text_extractor_plan_sku                = string
    text_extractor_always_ready_instances  = number
    text_extractor_maximum_scale_out_limit = number
    text_extractor_plan_maximum_burst      = number
    pdf_redactor_service_plan_sku          = string
    pdf_redactor_always_ready_instances    = number
    pdf_redactor_maximum_scale_out_limit   = number
    pdf_redactor_plan_maximum_burst        = number
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

variable "overnight_clear_down" {
  type = object({
    disabled      = number
    schedule      = string
  })
}

variable "sliding_clear_down" {
  type = object({
    disabled        = number
    look_back_hours = number
    protect_blobs   = bool
    schedule        = string
    batch_size      = number
  })
}

variable "hte_feature_flag" {
  type = bool
}

variable "image_conversion_redaction" {
  type = object({
    resolution      = number
    quality_percent = number
  })
}

variable "search_service_config" {
  type = object({
    replica_count                 = number
    partition_count               = number
    is_dynamic_throttling_enabled = bool
  })
}