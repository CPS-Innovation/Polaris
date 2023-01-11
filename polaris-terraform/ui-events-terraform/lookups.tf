data "azurerm_linux_function_app" "fa_gateway" {
  name = "fa-${local.gateway_resource_name}"
  resource_group_name = var.env != "prod" ? "rg-${var.polaris_resource_name_prefix}-${var.env}" : "rg-${var.polaris_resource_name_prefix}"
}

data "azurerm_storage_container" "pipeline_storage_container" {
  name                  = "documents"
  storage_account_name  = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
}