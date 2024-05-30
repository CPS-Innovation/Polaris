variable "polaris_resource_name_prefix" {
  type    = string
  default = "polaris"
}

variable "networking_resource_name_prefix" {
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

variable "terraform_service_principal_display_name" {
  type = string
}