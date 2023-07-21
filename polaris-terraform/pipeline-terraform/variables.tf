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

variable "ip_restrictions" {
  type = list(object({
    action                    = optional(string)
    ip_address                = optional(string)
    name                      = optional(string)
    priority                  = optional(number)
    service_tag               = optional(string)
    virtual_network_subnet_id = optional(string)
    headers = optional(object({
      x_azure_fdid      = optional(list(string))
      x_fd_health_probe = optional(bool)
      x_forwarded_for   = optional(list(string))
      x_forwarded_host  = optional(list(string))
    }))
  }))
  default = []
}