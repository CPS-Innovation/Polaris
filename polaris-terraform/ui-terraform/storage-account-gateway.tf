resource "azurerm_storage_account" "sa_gateway" {
  #checkov:skip=CKV_AZURE_206:Ensure that Storage Accounts use replication
  #checkov:skip=CKV2_AZURE_38:Ensure soft-delete is enabled on Azure storage account
  #checkov:skip=CKV2_AZURE_1:Ensure storage for critical data are encrypted with Customer Managed Key
  #checkov:skip=CKV2_AZURE_21:Ensure Storage logging is enabled for Blob service for read requests
  #checkov:skip=CKV2_AZURE_40:Ensure storage account is not configured with Shared Key authorization
  name                = "sacps${var.env != "prod" ? var.env : ""}gateway"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location

  account_kind                    = "StorageV2"
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  enable_https_traffic_only       = true
  public_network_access_enabled   = false
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = true

  min_tls_version = "TLS1_2"

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

# PDF Generator Storage Account Private Endpoint and DNS Config
# Create Private Endpoint for Blobs
resource "azurerm_private_endpoint" "pipeline_sa_gateway_blob_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}gateway-blob-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_blob_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}gateway-blob-psc"
    private_connection_resource_id = azurerm_storage_account.sa_gateway.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}

# Create Private Endpoint for Tables
resource "azurerm_private_endpoint" "pipeline_sa_gateway_table_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}gateway-table-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_table_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}gateway-table-psc"
    private_connection_resource_id = azurerm_storage_account.sa_gateway.id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }
}

# Create Private Endpoint for Files
resource "azurerm_private_endpoint" "pipeline_sa_gateway_file_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}gateway-file-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_file_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}gateway-file-psc"
    private_connection_resource_id = azurerm_storage_account.sa_gateway.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }
}

# Create Private Endpoint for Queues
resource "azurerm_private_endpoint" "pipeline_sa_gateway_queue_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}gateway-queue-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_queue_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}gateway-queue-psc"
    private_connection_resource_id = azurerm_storage_account.sa_gateway.id
    is_manual_connection           = false
    subresource_names              = ["queue"]
  }
}

resource "azapi_resource" "polaris_sa_gateway_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-gateway-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sa_gateway.name}/fileServices/default"
}

resource "azapi_resource" "polaris_sa_gateway_file_share_staging1" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-gateway-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sa_gateway.name}/fileServices/default"
}