resource "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  address_space       = [var.vnetAddressSpace]
  
  tags = {
    environment = var.environment.name
  }
}