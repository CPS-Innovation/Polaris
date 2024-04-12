#################### Resource Group ####################
resource "azurerm_resource_group" "rg_polaris_workspace" {
  name     = var.environment.alias != "prod" ? "rg-${var.app_name_prefix}-analytics-${var.environment.alias}" : "rg-${var.app_name_prefix}-analytics"
  location = var.location
  tags     = local.common_tags
}