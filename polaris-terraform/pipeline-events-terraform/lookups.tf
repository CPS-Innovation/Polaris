data "azurerm_search_service" "ss" {
  name                = "ss-${local.resource_name}"
  resource_group_name = "rg-${local.resource_name}"
}

data "azurerm_function_app" "fa_text_extractor" {
  name = "fa-${local.resource_name}-text-extractor"
  resource_group_name = "rg-${local.resource_name}"
}

data "azurerm_eventgrid_system_topic" "pipeline_document_deleted_topic" {
  name                = "pipeline-document-deleted-${var.env != "prod" ? var.env : ""}-topic"
  resource_group_name = "rg-${local.resource_name}"
}