resource "azurerm_cognitive_account" "computer_vision_service" {
  #checkov:skip=CKV2_AZURE_22:Ensure that Cognitive Services enables customer-managed key for encryption
  #checkov:skip=CKV_AZURE_236:Ensure that Cognitive Services accounts enable local authentication
  name                = "cv-${local.pipeline_resource_name}"
  resource_group_name = azurerm_resource_group.rg_polaris_pipeline.name
  location            = azurerm_resource_group.rg_polaris_pipeline.location
  kind                = "ComputerVision"

  sku_name                           = "S1"
  custom_subdomain_name              = "polaris${var.env}"
  outbound_network_access_restricted = false
  public_network_access_enabled      = false
  dynamic_throttling_enabled         = var.search_service_config.is_dynamic_throttling_enabled

  network_acls {
    default_action = "Deny"
    virtual_network_rules {
      subnet_id = data.azurerm_subnet.polaris_apps_subnet.id
    }
    virtual_network_rules {
      subnet_id = data.azurerm_subnet.polaris_sa_subnet.id
    }
    virtual_network_rules {
      subnet_id = data.azurerm_subnet.polaris_textextractor_2_subnet.id
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_computer_vision_service_pe" {
  name                = "${azurerm_cognitive_account.computer_vision_service.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_pipeline.name
  location            = azurerm_resource_group.rg_polaris_pipeline.location
  subnet_id           = data.azurerm_subnet.polaris_sa_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_cognitive_account.id]
  }

  private_service_connection {
    name                           = "${azurerm_cognitive_account.computer_vision_service.name}-psc"
    private_connection_resource_id = azurerm_cognitive_account.computer_vision_service.id
    is_manual_connection           = false
    subresource_names              = ["account"]
  }
}