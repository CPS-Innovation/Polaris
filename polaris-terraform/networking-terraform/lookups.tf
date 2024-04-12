data "azurerm_resource_group" "rg_networking" {
  name = "rg-${var.resource_name_prefix}"
}

data "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.root_alias}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}

data "azurerm_route_table" "env_route_table" {
  name                = var.environment.root_alias == "uat" ? "uks-rt-innovation${var.environment.root_alias}" : "uks-rt-vnet-innovation-${var.environment.root_alias}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}