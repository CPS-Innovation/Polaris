#################### Resource Group ####################

resource "azurerm_resource_group" "rg" {
  name     = "rg-${local.resource_name}"
  location = "UK South"
  tags     = local.common_tags
}

resource "azurerm_resource_group" "rg_mv" {
  count = var.env == "prod" ? 1 : 0

  name     = "rg-${local.resource_name}-mv"
  location = "UK South"
  tags     = local.common_tags
}
