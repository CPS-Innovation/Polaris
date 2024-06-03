resource "azurerm_storage_account" "sacpspolaris" {
  #checkov:skip=CKV2_AZURE_38:Ensure soft-delete is enabled on Azure storage account
  #checkov:skip=CKV_AZURE_21:Ensure Storage logging is enabled for Blob service for read requests
  #checkov:skip=CKV2_AZURE_1:Ensure storage for critical data are encrypted with Customer Managed Key
  #checkov:skip=CKV2_AZURE_40:Ensure storage account is not configured with Shared Key authorization
  #checkov:skip=CKV2_AZURE_50:Ensure Azure Storage Account storing Machine Learning workspace high business impact data is not publicly accessible
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location

  account_kind                    = "StorageV2"
  account_replication_type        = "RAGRS"
  account_tier                    = "Standard"
  enable_https_traffic_only       = true
  min_tls_version                 = "TLS1_2"
  public_network_access_enabled   = false
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = true

  network_rules {
    default_action = "Deny"
    bypass         = ["Metrics", "Logging", "AzureServices"]
    virtual_network_subnet_ids = [
      data.azurerm_subnet.polaris_ci_subnet.id,
      data.azurerm_subnet.polaris_ui_subnet.id,
      data.azurerm_subnet.polaris_gateway_subnet.id,
      data.azurerm_subnet.polaris_proxy_subnet.id,
      data.azurerm_subnet.polaris_apps_subnet.id,
      data.azurerm_subnet.polaris_apps2_subnet.id
    ]
  }

  queue_properties {
    logging {
      delete                = true
      read                  = true
      write                 = true
      version               = "1.0"
      retention_policy_days = 10
    }

    hour_metrics {
      enabled               = true
      include_apis          = true
      version               = "1.0"
      retention_policy_days = 10
    }

    minute_metrics {
      enabled               = true
      include_apis          = true
      version               = "1.0"
      retention_policy_days = 10
    }
  }

  sas_policy {
    expiration_period = "0.0:05:00"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags
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

resource "azapi_resource" "polaris_sacpspolaris_proxy_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-proxy-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpspolaris.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpspolaris]
}

resource "azapi_resource" "polaris_sacpspolaris_ui_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-ui-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpspolaris.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpspolaris]
}

resource "azapi_resource" "polaris_sacpspolaris_ui_staging1_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-ui-content-share-1"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sacpspolaris.name}/fileServices/default"

  depends_on = [azurerm_storage_account.sacpspolaris]
}

resource "azurerm_storage_container" "polaris_proxy_content" {
  #checkov:skip=CKV2_AZURE_21:Ensure Storage logging is enabled for Blob service for read requests
  name                  = "content"
  storage_account_name  = azurerm_storage_account.sacpspolaris.name
  container_access_type = "private"

  depends_on = [azurerm_storage_account.sacpspolaris]
}
