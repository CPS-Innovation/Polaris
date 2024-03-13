resource "azurerm_linux_function_app_slot" "fa_coordinator_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_linux_function_app.fa_coordinator.id
  storage_account_name          = azurerm_storage_account.sa_coordinator.name
  storage_account_access_key    = azurerm_storage_account.sa_coordinator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_coordinator_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobs.ResetDurableState.Disabled"         = var.overnight_clear_down.disabled
    "AzureWebJobs.SlidingCaseClearDown.Disabled"      = var.sliding_clear_down.disabled
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_coordinator.primary_connection_string
    "BlobExpirySecs"                                  = 3600
    "BlobServiceContainerName"                        = "documents"
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobUserDelegationKeyExpirySecs"                 = 3600
    "CoordinatorOrchestratorTimeoutSecs"              = "600"
    "CoordinatorTaskHub"                              = "fapolaris${var.env != "prod" ? var.env : ""}coordinatorstaging1"
    "DdeiBaseUrl"                                     = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                                   = "" #set by script
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "HostType"                                        = "Staging1"
    "OvernightClearDownSchedule"                      = var.overnight_clear_down.schedule
    "PolarisPipelineRedactPdfBaseUrl"                 = "https://fa-${local.global_name}-pdf-generator.azurewebsites.net/api/"
    "PolarisPipelineRedactPdfFunctionAppKey"          = "" #set by script
    # "PolarisPipelineRedactorPdfBaseUrl"             = https://fa-${local.global_name}-pdf-redactor.azurewebsites.net/api/
    # "PolarisPipelineRedactorPdfFunctionAppKey"      = "" #set by script
    "PolarisPipelineRedactorPdfBaseUrl"               = "https://fa-${local.global_name}-pdf-generator.azurewebsites.net/api/"
    "PolarisPipelineRedactorPdfFunctionAppKey"        = "" #set by script
    "PolarisPipelineTextExtractorBaseUrl"             = "https://fa-${local.global_name}-text-extractor.azurewebsites.net/api/"
    "PolarisPipelineTextExtractorFunctionAppKey"      = "" #set by script
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.pipeline_logging.coordinator_scale_controller
    "SlidingClearDownInputHours"                      = var.sliding_clear_down.look_back_hours
    "SlidingClearDownProtectBlobs"                    = var.sliding_clear_down.protect_blobs
    "SlidingClearDownSchedule"                        = var.sliding_clear_down.schedule
    "SlidingClearDownBatchSize"                       = var.sliding_clear_down.batch_size
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_coordinator.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_coordinator_file_share_staging1.name
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"             = "10"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITE_WARMUP_PATH"                             = "/api/status"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    elastic_instance_minimum               = var.pipeline_component_service_plans.coordinator_always_ready_instances
    app_scale_limit                        = var.pipeline_component_service_plans.coordinator_maximum_scale_out_limit
    runtime_scale_monitoring_enabled       = true
    application_stack {
      dotnet_version = "6.0"
    }
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
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
resource "azurerm_private_endpoint" "pipeline_coordinator_staging1_pe" {
  name                = "${azurerm_linux_function_app.fa_coordinator.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_coordinator.name}-staging1-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_coordinator.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}