#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "polaris"
}

variable "networking_resource_name_suffix" {
  default = "networking"
}

variable "env" {
  type = string 
}

variable "location" {
  description = "The location of this resource"
  type        = string
}

variable "app_service_plan_web_sku" {
  type = string
}

variable "environment_tag" {
  type        = string
  description = "Environment tag value"
}

variable "polaris_webapp_details" {
  type = object({
    valid_audience = string
    valid_scopes = string
	valid_roles = string
  })
}