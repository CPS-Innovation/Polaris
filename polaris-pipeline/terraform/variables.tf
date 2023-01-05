#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "rumpole-pipeline"
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

variable "queue_config" {
  type = object({
    update_search_index_by_version_queue_name = string
    update_search_index_by_blob_name_queue_name = string
  })
}
