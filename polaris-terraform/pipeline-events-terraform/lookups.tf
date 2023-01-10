data "azurerm_search_service" "ss" {
  name                = "ss-${local.pipeline_resource_name}"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_linux_function_app" "fa_gateway" {
  name = "fa-${local.gateway_resource_name}"
  resource_group_name = var.env != "prod" ? "rg-${var.polaris_resource_name_prefix}-${var.env}" : "rg-${var.polaris_resource_name_prefix}"
}

data "azurerm_windows_function_app" "fa_pdf_generator" {
  name = "fa-${local.pipeline_resource_name}-pdf-generator"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_linux_function_app" "fa_text_extractor" {
  name = "fa-${local.pipeline_resource_name}-text-extractor"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_eventgrid_system_topic" "pipeline_document_deleted_topic" {
  name                = "pipeline-document-deleted-${var.env != "prod" ? var.env : ""}-topic"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_storage_container" "pipeline_storage_container" {
  name                  = "documents"
  storage_account_name  = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
}