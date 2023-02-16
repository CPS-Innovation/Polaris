resource "azurerm_cognitive_account" "computer_vision_service" {
  name                = "cv-${local.resource_name}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  kind                = "ComputerVision"

  sku_name                           = "S1"
  custom_subdomain_name              = "polaris${var.env}"
  outbound_network_access_restricted = false
  public_network_access_enabled      = false

  network_acls {
    default_action = "Deny"
    virtual_network_rules {
      subnet_id = data.azurerm_subnet.polaris_textextractor_subnet.id
    }
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_computer_vision_service_pe" {
  name                = "${azurerm_cognitive_account.computer_vision_service.name}-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
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

# Create DNS A Record
resource "azurerm_private_dns_a_record" "pipeline_computer_vision_service_dns_a" {
  name                = azurerm_cognitive_account.computer_vision_service.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_cognitive_account.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_computer_vision_service_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}