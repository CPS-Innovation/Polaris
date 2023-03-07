resource "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  address_space       = [var.vnetAddressSpace]
  dns_servers         = ["10.2.64.10", "10.3.64.10"]

  tags = {
    environment = var.environment.name
  }
}