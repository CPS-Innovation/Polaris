data "azurerm_resource_group" "rg_networking" {
  name = "rg-${var.resource_name_prefix}"
}

data "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.name}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}

data "azurerm_route_table" "env_route_table" {
  name                = var.environment.alias == "uat" ? "uks-rt-innovation${var.environment.alias}" : "uks-rt-vnet-innovation-${var.environment.name}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}