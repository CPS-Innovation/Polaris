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

variable "environment_tag" {
  type        = string
  description = "Environment tag value"
}

variable "networking_resource_name_suffix" {
  default = "networking"
}

variable "terraform_service_principal_display_name" {
  type = string
}