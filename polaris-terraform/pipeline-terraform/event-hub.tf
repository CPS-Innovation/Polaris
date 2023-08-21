resource "azurerm_eventhub_namespace" "polaris-eh-namespace" {
  name = "eh-${local.resource_name}-namespace"
  location                      = azurerm_resource_group.rg.location
  resource_group_name           = azurerm_resource_group.rg.name
  sku = var.pipeline_event_hub_settings.sku
  capacity = var.pipeline_event_hub_settings.capacity
  public_network_access_enabled = false
  
  network_rulesets {
    default_action                 = "Deny"
    public_network_access_enabled  = false
    trusted_service_access_enabled = true
    virtual_network_rule {
      subnet_id = data.azurerm_subnet.polaris_netherite_subnet.id
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_eh_namespace_pe" {
  name                = "${azurerm_eventhub_namespace.polaris-eh-namespace.name}-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_netherite_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_event_hub_namespace.id]
  }

  private_service_connection {
    name                           = "${azurerm_eventhub_namespace.polaris-eh-namespace.name}-psc"
    private_connection_resource_id = azurerm_eventhub_namespace.polaris-eh-namespace.id
    is_manual_connection           = false
    subresource_names              = ["namespace"]
  }
}