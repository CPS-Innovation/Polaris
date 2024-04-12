data "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.root_alias}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}

data "azurerm_route_table" "env_route_table" {
  name                = "uks-rt-vnet-innovation-${var.environment.root_alias}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}