#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "polaris-pipeline"
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

variable "ddei_config" {
  type = object({
    base_url = string
    list_documents_function_key = string
    get_document_function_key = string
  })
}
