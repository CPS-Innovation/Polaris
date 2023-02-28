resource "azurerm_private_dns_resolver" "polaris_private_dns_resolver" {
  name                = "polaris-dns-resolver"
  resource_group_name = azurerm_resource_group.rg_networking.name
  location            = azurerm_resource_group.rg_networking.location
  virtual_network_id  = azurerm_virtual_network.vnet_networking.id

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_private_dns_resolver_inbound_endpoint" "polaris_private_dns_resolver_inbound_endpoint" {
  name                    = "polaris-dns-resolve-inbound"
  private_dns_resolver_id = azurerm_private_dns_resolver.polaris_private_dns_resolver.id
  location                = azurerm_private_dns_resolver.polaris_private_dns_resolver.location
  ip_configurations {
    private_ip_allocation_method = "Dynamic"
    subnet_id                    = azurerm_subnet.sn_polaris_dns_resolve_subnet.id
  }

  depends_on = [azurerm_private_dns_resolver.polaris_private_dns_resolver]
  tags = {
    environment = var.environment.name
  }
}