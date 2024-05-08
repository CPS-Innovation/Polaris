resource "azurerm_storage_account" "sa_pdf_redactor" {
  #checkov:skip=CKV_AZURE_206:Ensure that Storage Accounts use replication
  #checkov:skip=CKV2_AZURE_38:Ensure soft-delete is enabled on Azure storage account
  #checkov:skip=CKV2_AZURE_1:Ensure storage for critical data are encrypted with Customer Managed Key
  #checkov:skip=CKV2_AZURE_21:Ensure Storage logging is enabled for Blob service for read requests
  #checkov:skip=CKV2_AZURE_40:Ensure storage account is not configured with Shared Key authorization
  name                = "sacps${var.env != "prod" ? var.env : ""}pdfredactor"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

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
      data.azurerm_subnet.polaris_pdfredactor_subnet.id,
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

# PDF Redactor Storage Account Private Endpoint and DNS Config
# Create Private Endpoint for Blobs
resource "azurerm_private_endpoint" "pipeline_sa_pdf_redactor_blob_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-blob-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_blob_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-blob-psc"
    private_connection_resource_id = azurerm_storage_account.sa_pdf_redactor.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}

# Create Private Endpoint for Tables
resource "azurerm_private_endpoint" "pipeline_sa_pdf_redactor_table_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-table-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_table_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-table-psc"
    private_connection_resource_id = azurerm_storage_account.sa_pdf_redactor.id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }
}

# Create Private Endpoint for Files
resource "azurerm_private_endpoint" "pipeline_sa_pdf_redactor_file_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-file-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_file_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-file-psc"
    private_connection_resource_id = azurerm_storage_account.sa_pdf_redactor.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }
}

# Create Private Endpoint for Queues
resource "azurerm_private_endpoint" "pipeline_sa_pdf_redactor_queue_pe" {
  name                = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-queue-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_queue_storage.id]
  }

  private_service_connection {
    name                           = "sacps${var.env != "prod" ? var.env : ""}pdfredactor-queue-psc"
    private_connection_resource_id = azurerm_storage_account.sa_pdf_redactor.id
    is_manual_connection           = false
    subresource_names              = ["queue"]
  }
}

resource "azapi_resource" "pipeline_sa_pdf_redactor_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "pipeline-pdf-redactor-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sa_pdf_redactor.name}/fileServices/default"
}

resource "azapi_resource" "pipeline_sa_pdf_redactor_file_share_staging1" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "pipeline-pdf-redactor-content-share-1"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sa_pdf_redactor.name}/fileServices/default"
}