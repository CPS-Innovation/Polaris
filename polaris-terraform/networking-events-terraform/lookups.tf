data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment.alias}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}

data "azurerm_subnet" "sn_gateway_subnet" {
  name                 = "GatewaySubnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.resource_name_prefix}"
}