#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "polaris-pipeline"
}

variable "polaris_resource_name_prefix" {
  type = string
  default = "polaris"
}

variable "env" {
  type = string 
}

variable "app_service_plan_sku" {
  type = object({
    tier = string
    size = string
  })
}
