resource "azurerm_private_dns_resolver" "polaris_dns_resolver" {
  name                = "polaris-dns-resolver"
  resource_group_name = data.azurerm_resource_group.rg_networking.name
  location            = data.azurerm_resource_group.rg_networking.location
  virtual_network_id  = data.azurerm_virtual_network.vnet_networking.id

  tags = local.common_tags
}

resource "azurerm_private_dns_resolver_inbound_endpoint" "polaris_dns_resolver_inbound_endpoint" {
  name                    = "polaris-dns-resolve-inbound"
  private_dns_resolver_id = azurerm_private_dns_resolver.polaris_dns_resolver.id
  location                = azurerm_private_dns_resolver.polaris_dns_resolver.location

  ip_configurations {
    private_ip_allocation_method = "Dynamic"
    subnet_id                    = azurerm_subnet.sn_polaris_dns_resolver_inbound_subnet.id
  }

  tags = local.common_tags

  depends_on = [
    azurerm_private_dns_resolver.polaris_dns_resolver,
    azurerm_subnet.sn_polaris_dns_resolver_inbound_subnet
  ]
}

resource "azurerm_private_dns_resolver_outbound_endpoint" "polaris_dns_resolver_outbound_endpoint" {
  name                    = "polaris-dns-resolve-outbound"
  private_dns_resolver_id = azurerm_private_dns_resolver.polaris_dns_resolver.id
  location                = azurerm_private_dns_resolver.polaris_dns_resolver.location
  subnet_id               = azurerm_subnet.sn_polaris_dns_resolver_outbound_subnet.id

  tags = local.common_tags

  depends_on = [
    azurerm_private_dns_resolver.polaris_dns_resolver,
    azurerm_subnet.sn_polaris_dns_resolver_outbound_subnet
  ]
}

resource "azurerm_private_dns_resolver_dns_forwarding_ruleset" "polaris_dns_resolver_forwarding_ruleset" {
  name                                       = "polaris-dns-resolver-forwarding-ruleset"
  resource_group_name                        = data.azurerm_resource_group.rg_networking.name
  location                                   = data.azurerm_resource_group.rg_networking.location
  private_dns_resolver_outbound_endpoint_ids = [azurerm_private_dns_resolver_outbound_endpoint.polaris_dns_resolver_outbound_endpoint.id]

  tags = local.common_tags

  depends_on = [
    azurerm_private_dns_resolver.polaris_dns_resolver,
    azurerm_private_dns_resolver_outbound_endpoint.polaris_dns_resolver_outbound_endpoint
  ]
}

resource "azurerm_private_dns_resolver_virtual_network_link" "polaris_dns_resolver_vnet_link" {
  name                      = "vnet-innovation-${var.environment.name}-link"
  dns_forwarding_ruleset_id = azurerm_private_dns_resolver_dns_forwarding_ruleset.polaris_dns_resolver_forwarding_ruleset.id
  virtual_network_id        = data.azurerm_virtual_network.vnet_networking.id

  depends_on = [
    azurerm_private_dns_resolver_dns_forwarding_ruleset.polaris_dns_resolver_forwarding_ruleset
  ]
}

resource "azurerm_private_dns_resolver_forwarding_rule" "polaris_dns_resolver_forwarding_rule" {
  name                      = "CpsPrivate"
  dns_forwarding_ruleset_id = azurerm_private_dns_resolver_dns_forwarding_ruleset.polaris_dns_resolver_forwarding_ruleset.id
  domain_name               = "cps.gov.uk."
  enabled                   = true
  target_dns_servers {
    ip_address = "10.8.0.6"
    port       = 53
  }
  target_dns_servers {
    ip_address = "10.8.0.7"
    port       = 53
  }

  depends_on = [
    azurerm_private_dns_resolver_dns_forwarding_ruleset.polaris_dns_resolver_forwarding_ruleset
  ]
}