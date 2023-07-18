resource "azurerm_virtual_network" "vnet_networking" {
  #checkov:skip=CKV2_AZURE_182:Ensure that VNET has at least 2 connected DNS Endpoints
  name                = "vnet-innovation-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  address_space       = [var.vnetAddressSpace]
  dns_servers         = [var.vnetDnsServer]

  tags = {
    environment = var.environment.name
  }
}