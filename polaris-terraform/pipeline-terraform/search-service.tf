resource "azurerm_search_service" "ss" {
  name                          = "ss-${local.resource_name}"
  resource_group_name           = azurerm_resource_group.rg.name
  location                      = azurerm_resource_group.rg.location
  sku                           = "standard"
  replica_count                 = 3
  public_network_access_enabled = false
  tags                          = local.common_tags
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_search_service_pe" {
  name                = "${azurerm_search_service.ss.name}-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_sa_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_search_service.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_search_service.id]
  }

  private_service_connection {
    name                           = "${azurerm_search_service.ss.name}-psc"
    private_connection_resource_id = azurerm_search_service.ss.id
    is_manual_connection           = false
    subresource_names              = ["searchService"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "pipeline_search_service_dns_a" {
  name                = azurerm_search_service.ss.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_search_service.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_search_service_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}