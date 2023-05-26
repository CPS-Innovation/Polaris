resource "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  address_space       = [var.vnetAddressSpace]
  dns_servers = [
    azurerm_private_dns_resolver_inbound_endpoint.polaris_private_dns_resolver_inbound_endpoint.ip_configurations[0].private_ip_address
  ]

  tags = {
    environment = var.environment.name
  }
  
  depends_on = [
    azurerm_private_dns_resolver.polaris_private_dns_resolver,
    azurerm_private_dns_resolver_inbound_endpoint.polaris_private_dns_resolver_inbound_endpoint
  ]
}