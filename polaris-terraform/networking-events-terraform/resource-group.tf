#################### Resource Group ####################

resource "azurerm_resource_group" "rg_networking" {
  name     = "rg-${var.resource_name_prefix}"
  location = var.location
  tags     = local.common_tags
}