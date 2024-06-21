#################### Resource Group ####################

resource "azurerm_resource_group" "rg" {
  name     = "rg-${local.resource_name}"
  location = "UK South"
  tags     = local.common_tags
}

resource "azurerm_resource_group" "rg_mv" {
  name     = "rg-polaris-mv-plans${local.resource_suffix}"
  location = "UK South"
  tags     = local.common_tags
}
