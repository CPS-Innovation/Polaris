#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "polaris"
}

variable "env" {
  type = string 
}

variable "location" {
  description = "The location of this resource"
  type        = string
}

variable "app_service_plan_sku" {
  type = object({
    tier = string
    size = string
  })
}

variable "core_data_api_details" {
  type = object({
    api_id = string
    api_url = string
    api_scope = string
    case_confirm_user_impersonation_id = string
  })
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

variable "stub_blob_storage_connection_string" {
  type = string
}