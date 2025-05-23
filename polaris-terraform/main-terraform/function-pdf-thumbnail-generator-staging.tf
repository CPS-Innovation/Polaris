resource "azurerm_windows_function_app_slot" "fa_pdf_thumbnail_generator_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_windows_function_app.fa_pdf_thumbnail_generator.id
  storage_account_name          = azurerm_storage_account.sa_pdf_thumbnail_generator.name
  storage_account_access_key    = azurerm_storage_account.sa_pdf_thumbnail_generator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_pdfthumbnailgenerator_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_pdf_thumbnail_generator.primary_connection_string
    "AzureWebJobs.SlidingClearDown.Disabled"          = 1
    "BlobServiceContainerName"                        = var.blob_service_container_name
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "SlidingClearDownBatchSize"                       = var.thumbnail_generator_sliding_clear_down.batch_size
    "SlidingClearDownSchedule"                        = var.thumbnail_generator_sliding_clear_down.schedule
    "SlidingClearDownInputHours"                      = var.thumbnail_generator_sliding_clear_down.input_hours
    "BlobServiceContainerNameThumbnails"              = var.blob_thumbnails_container_name
    "ThumbnailGeneratorTaskHub"                       = "fapolaris${var.env != "prod" ? var.env : ""}pdfthumbgenstaging1"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet-isolated"
    "HostType"                                        = "Staging1"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_pdf_thumbnail_generator.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_pdf_thumbnail_generator_file_share_staging1.name
    "WEBSITE_DNS_ALT_SERVER"                          = var.dns_alt_server
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "1"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"             = "10"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITE_WARMUP_PATH"                             = "/api/status"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"          = "1"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    always_on                              = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    application_stack {
      dotnet_version = "v8.0"
    }
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
    use_32_bit_worker                 = false
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = false
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "AllowAnonymous"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_thumbnail_generator_staging1_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_thumbnail_generator.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg_coordinator.name
  location            = azurerm_resource_group.rg_coordinator.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_thumbnail_generator.name}-staging1-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_thumbnail_generator.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }

  depends_on = [
    azurerm_windows_function_app_slot.fa_pdf_thumbnail_generator_staging1
  ]
}