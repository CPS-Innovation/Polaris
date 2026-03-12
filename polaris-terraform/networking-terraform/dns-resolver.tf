resource "azurerm_private_dns_resolver" "polaris_dns_resolver" {
  name                = "polaris-dns-resolver"
  resource_group_name = data.azurerm_resource_group.rg_networking.name
  location            = data.azurerm_resource_group.rg_networking.location
  virtual_network_id  = data.azurerm_virtual_network.vnet_networking.id

  tags = local.common_tags
}