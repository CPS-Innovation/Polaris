#################### Variables ####################

variable "polaris_resource_name_prefix" {
  type    = string
  default = "polaris"
}

variable "pipeline_resource_name_prefix" {
  type    = string
  default = "polaris-pipeline"
}

variable "env" {
  type = string
}

variable "terraform_service_principal_display_name" {
  type = string
}