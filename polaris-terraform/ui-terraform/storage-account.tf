resource "azurerm_storage_account" "sacpspolaris" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location

  account_kind              = "StorageV2"
  account_replication_type  = "RAGRS"
  account_tier              = "Standard"
  enable_https_traffic_only = true
  min_tls_version           = "TLS1_2"

  tags = local.common_tags
}

resource "azurerm_storage_account_network_rules" "polaris_sacpspolaris_rules" {
  storage_account_id = azurerm_storage_account.sacpspolaris.id

  default_action = "Deny"
  bypass         = ["Metrics", "Logging", "AzureServices"]
  depends_on     = [azurerm_storage_account.sacpspolaris]
  virtual_network_subnet_ids = [
    data.azurerm_subnet.polaris_ci_subnet.id,
    data.azurerm_subnet.polaris_gateway_subnet.id,
    data.azurerm_subnet.polaris_proxy_subnet.id,
    data.azurerm_subnet.polaris_auth_handover_subnet
  ]
}

# Create Private Endpoint for Blobs
resource "azurerm_private_endpoint" "polaris_sacpspolaris_blob_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris-blob-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_blob_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}polaris-blob-psc"
    private_connection_resource_id = azurerm_storage_account.sacpspolaris.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_sacpspolaris_blob_dns_a" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_blob_storage.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_sacpspolaris_blob_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}

# Create Private Endpoint for Tables
resource "azurerm_private_endpoint" "polaris_sacpspolaris_table_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris-table-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_table_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}polaris-table-psc"
    private_connection_resource_id = azurerm_storage_account.sacpspolaris.id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_sacpspolaris_table_dns_a" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_table_storage.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_sacpspolaris_table_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}

# Create Private Endpoint for Files
resource "azurerm_private_endpoint" "polaris_sacpspolaris_file_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris-file-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_file_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}polaris-file-psc"
    private_connection_resource_id = azurerm_storage_account.sacpspolaris.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_sacpspolaris_file_dns_a" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_file_storage.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_sacpspolaris_file_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}

resource "azapi_resource" "polaris_sacpspolaris_gateway_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-gateway-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpspolaris.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpspolaris]
}

resource "azapi_resource" "polaris_sacpspolaris_proxy_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-proxy-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpspolaris.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpspolaris]
}

resource "azapi_resource" "polaris_sacpspolaris_auth_handover_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-auth-handover-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpspolaris.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpspolaris]
}

resource "azurerm_storage_container" "polaris_proxy_content" {
  name                  = "content"
  storage_account_name  = azurerm_storage_account.sacpspolaris.name
  container_access_type = "private"

  depends_on = [azurerm_storage_account.sacpspolaris]
}
