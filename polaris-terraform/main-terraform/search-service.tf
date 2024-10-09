resource "azurerm_search_service" "ss" {
  #checkov:skip=CKV_AZURE_207:Ensure Azure Cognitive Search service uses managed identities to access Azure resources
  name                          = "ss-${local.pipeline_resource_name}"
  resource_group_name           = azurerm_resource_group.rg_polaris_pipeline.name
  location                      = azurerm_resource_group.rg_polaris_pipeline.location
  sku                           = "standard"
  replica_count                 = var.search_service_config.replica_count
  partition_count               = var.search_service_config.partition_count
  public_network_access_enabled = false
  tags                          = local.common_tags
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_search_service_pe" {
  name                = "${azurerm_search_service.ss.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_pipeline.name
  location            = azurerm_resource_group.rg_polaris_pipeline.location
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

  depends_on = [azurerm_search_service.ss]
}