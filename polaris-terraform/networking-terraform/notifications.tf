resource "azurerm_storage_account" "sa_alert_processing" {
  #checkov:skip=CKV_AZURE_206:Ensure that Storage Accounts use replication
  #checkov:skip=CKV2_AZURE_38:Ensure soft-delete is enabled on Azure storage account
  #checkov:skip=CKV2_AZURE_1:Ensure storage for critical data are encrypted with Customer Managed Key
  #checkov:skip=CKV2_AZURE_21:Ensure Storage logging is enabled for Blob service for read requests
  #checkov:skip=CKV2_AZURE_40:Ensure storage account is not configured with Shared Key authorization
  #checkov:skip=CKV2_AZURE_41:Ensure storage account is configured with SAS expiration policy
  #checkov:skip=CKV2_AZURE_50:Ensure Azure Storage Account storing Machine Learning workspace high business impact data is not publicly accessible
  count = var.environment.alias == "prod" ? 1 : 0

  name                            = "sacps${local.env_name}alertprocessing"
  resource_group_name             = azurerm_resource_group.rg_polaris_workspace.name
  location                        = azurerm_resource_group.rg_polaris_workspace.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  enable_https_traffic_only       = true
  public_network_access_enabled   = true
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = true

  min_tls_version = "TLS1_2"

  network_rules {
    default_action = "Deny"
    bypass         = ["Metrics", "Logging", "AzureServices"]
    virtual_network_subnet_ids = [
      azurerm_subnet.sn_polaris_alert_notifications_subnet.id
    ]
  }

  routing {
    publish_microsoft_endpoints = true
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags
}

# Create Private Endpoint for Tables
resource "azurerm_private_endpoint" "alert_processing_sa_table_pe" {
  count = var.environment.alias == "prod" ? 1 : 0

  name                = "sacps${local.env_name}alertprocessing-table-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_pipeline_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_table_storage.id]
  }

  private_service_connection {
    name                           = "sacps${local.env_name}alertprocessing-table-psc"
    private_connection_resource_id = azurerm_storage_account.sa_alert_processing[0].id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }
}

# Create Private Endpoint for Files
resource "azurerm_private_endpoint" "alert_processing_sa_file_pe" {
  count = var.environment.alias == "prod" ? 1 : 0

  name                = "sacps${local.env_name}alertprocessing-file-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_pipeline_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_file_storage.id]
  }

  private_service_connection {
    name                           = "sacps${local.env_name}alertprocessing-file-psc"
    private_connection_resource_id = azurerm_storage_account.sa_alert_processing[0].id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }
}

# Create Private Endpoint for Queues
resource "azurerm_private_endpoint" "alert_processing_sa_queue_pe" {
  count = var.environment.alias == "prod" ? 1 : 0

  name                = "sacps${local.env_name}alertprocessing-queue-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_pipeline_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_queue_storage.id]
  }

  private_service_connection {
    name                           = "sacps${local.env_name}alertprocessing-queue-psc"
    private_connection_resource_id = azurerm_storage_account.sa_alert_processing[0].id
    is_manual_connection           = false
    subresource_names              = ["queue"]
  }
}

resource "azapi_resource" "alert_processing_sa_file_share" {
  count = var.environment.alias == "prod" ? 1 : 0

  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "alert-processor-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${azurerm_resource_group.rg_polaris_workspace.name}/providers/Microsoft.Storage/storageAccounts/${azurerm_storage_account.sa_alert_processing[0].name}/fileServices/default"
}

resource "azurerm_service_plan" "asp_alert_notifications" {
  count = var.environment.alias == "prod" ? 1 : 0

  name                = "asp-alert-notifications${local.env_name_suffix}"
  location            = azurerm_resource_group.rg_polaris_workspace.location
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  os_type             = "Windows"
  sku_name            = "WS1"

  tags = local.common_tags
}

resource "azurerm_logic_app_standard" "alert_notifications_processor" {
  count = var.environment.alias == "prod" ? 1 : 0

  name                       = "send-alert-teams${local.env_name_suffix}"
  location                   = azurerm_resource_group.rg_polaris_workspace.location
  resource_group_name        = azurerm_resource_group.rg_polaris_workspace.name
  app_service_plan_id        = azurerm_service_plan.asp_alert_notifications[0].id
  storage_account_name       = azurerm_storage_account.sa_alert_processing[0].name
  storage_account_access_key = azurerm_storage_account.sa_alert_processing[0].primary_access_key
  virtual_network_subnet_id  = azurerm_subnet.sn_polaris_alert_notifications_subnet.id
  https_only                 = true
  version                    = "~4"

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "node"
    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.ai_polaris.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING"    = azurerm_application_insights.ai_polaris.connection_string
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sa_alert_processing[0].primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.alert_processing_sa_file_share[0].name
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_RUN_FROM_PACKAGE"                 = "1"
    "WEBSITE_NODE_DEFAULT_VERSION"             = "~18"
  }

  site_config {
    use_32_bit_worker_process        = false
    always_on                        = false
    dotnet_framework_version         = "v4.0"
    ftps_state                       = "Disabled"
    pre_warmed_instance_count        = "0"
    app_scale_limit                  = "1"
    vnet_route_all_enabled           = true
    min_tls_version                  = "1.2"
    public_network_access_enabled    = true
    http2_enabled                    = true
    runtime_scale_monitoring_enabled = true
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_private_endpoint" "alert_notifications_processor_pe" {
  count = var.environment.alias == "prod" ? 1 : 0

  name                = "${azurerm_logic_app_standard.alert_notifications_processor[0].name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_logic_app_standard.alert_notifications_processor[0].name}-psc"
    private_connection_resource_id = azurerm_logic_app_standard.alert_notifications_processor[0].id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}