#################### Functions ####################

resource "azurerm_linux_function_app" "fa_coordinator" {
  name                          = "fa-${local.global_resource_name}-coordinator"
  location                      = azurerm_resource_group.rg_coordinator.location
  resource_group_name           = azurerm_resource_group.rg_coordinator.name
  service_plan_id               = azurerm_service_plan.asp_polaris_pipeline_coordinator.id
  storage_account_name          = azurerm_storage_account.sa_coordinator.name
  storage_account_access_key    = azurerm_storage_account.sa_coordinator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_coordinator_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  builtin_logging_enabled       = false

  app_settings = {
    "AzureFunctionsJobHost__extensions__durableTask__storageProvider__MaxQueuePollingInterval"     = var.coordinator.max_queue_polling_interval
    "AzureFunctionsJobHost__extensions__durableTask__storageProvider__ControlQueueBufferThreshold" = var.coordinator.control_queue_buffer_threshold
    "AzureFunctionsJobHost__extensions__durableTask__MaxConcurrentActivityFunctions"               = var.coordinator.max_concurrent_activity_functions
    "AzureFunctionsJobHost__extensions__durableTask__MaxConcurrentOrchestratorFunctions"           = var.coordinator.max_concurrent_orchestrator_functions
    "AzureWebJobs.ResetDurableState.Disabled"                                                      = var.overnight_clear_down.disabled
    "AzureWebJobs.SlidingCaseClearDown.Disabled"                                                   = var.sliding_clear_down.disabled
    "AzureWebJobsStorage"                                                                          = azurerm_storage_account.sa_coordinator.primary_connection_string
    # Bug 27315 - compiled coordinator builds arbitrarily stopped working unless a new "Storage" setting exists
    "Storage"                                         = azurerm_storage_account.sa_coordinator.primary_connection_string
    "BlobExpirySecs"                                  = 3600
    "BlobServiceContainerName"                        = var.blob_service_container_name
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobUserDelegationKeyExpirySecs"                 = 3600
    "ComputerVisionClientServiceKey"                  = azurerm_cognitive_account.computer_vision_service.primary_access_key
    "ComputerVisionClientServiceUrl"                  = azurerm_cognitive_account.computer_vision_service.endpoint
    "CoordinatorOrchestratorTimeoutSecs"              = "600"
    "CoordinatorTaskHub"                              = "fapolaris${var.env != "prod" ? var.env : ""}coordinator"
    "DdeiBaseUrl"                                     = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                                   = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet-isolated"
    "HostType"                                        = "Production"
    "MDSBaseUrl"                                      = "https://fa-${local.mds_resource_name}.azurewebsites.net"
    "MDSAccessKey"                                    = data.azurerm_key_vault_secret.kvs_fa_mds_host_keys.value
    "MDSMockBaseUrl"                                  = "https://as-${local.mds_mock_resource_name}.azurewebsites.net"
    "MDSMockAccessKey"                                = ""
    "LanguageServiceKey"                              = azurerm_cognitive_account.language_service.primary_access_key
    "LanguageServiceUrl"                              = azurerm_cognitive_account.language_service.endpoint
    "OvernightClearDownSchedule"                      = var.overnight_clear_down.schedule
    "PiiCategories"                                   = var.pii.categories
    "PiiChunkCharacterLimit"                          = var.pii.chunk_character_limit
    "PolarisPipelineRedactPdfBaseUrl"                 = "https://fa-${local.global_resource_name}-pdf-generator.azurewebsites.net/api/"
    "PolarisPipelineRedactorPdfBaseUrl"               = "https://fa-${local.global_resource_name}-pdf-redactor.azurewebsites.net/api/"
    "PolarisPipelineTextExtractorBaseUrl"             = "https://fa-${local.global_resource_name}-text-extractor.azurewebsites.net/api/"
    "SlidingClearDownInputHours"                      = var.sliding_clear_down.look_back_hours
    "SlidingClearDownProtectBlobs"                    = var.sliding_clear_down.protect_blobs
    "SlidingClearDownSchedule"                        = var.sliding_clear_down.schedule
    "SlidingClearDownBatchSize"                       = var.sliding_clear_down.batch_size
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_coordinator.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_coordinator_file_share.name
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
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
  }

  sticky_settings {
    app_setting_names = ["CoordinatorTaskHub", "HostType", "AzureWebJobs.SlidingCaseClearDown.Disabled"]
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    always_on                              = true
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
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
      app_settings["AzureWebJobs.ResetDurableState.Disabled"],
      app_settings["AzureWebJobs.SlidingCaseClearDown.Disabled"],
      app_settings["AzureWebJobsStorage"],
      app_settings["BlobExpirySecs"],
      app_settings["BlobServiceContainerName"],
      app_settings["BlobServiceUrl"],
      app_settings["BlobUserDelegationKeyExpirySecs"],
      app_settings["ComputerVisionClientServiceKey"],
      app_settings["ComputerVisionClientServiceUrl"],
      app_settings["CoordinatorOrchestratorTimeoutSecs"],
      app_settings["CoordinatorTaskHub"],
      app_settings["DdeiBaseUrl"],
      app_settings["DdeiAccessKey"],
      app_settings["LanguageServiceKey"],
      app_settings["LanguageServiceUrl"],
      app_settings["MDSBaseUrl"],
      app_settings["MDSAccessKey"],
      app_settings["MDSMockBaseUrl"],
      app_settings["MDSMockAccessKey"],      
      app_settings["OvernightClearDownSchedule"],
      app_settings["PiiChunkCharacterLimit"],
      app_settings["PolarisPipelineRedactPdfBaseUrl"],
      app_settings["PolarisPipelineRedactorPdfBaseUrl"],
      app_settings["PolarisPipelineTextExtractorBaseUrl"],
      app_settings["SlidingClearDownInputHours"],
      app_settings["SlidingClearDownProtectBlobs"],
      app_settings["SlidingClearDownSchedule"],
      app_settings["SlidingClearDownBatchSize"],
      app_settings["WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG"],
      app_settings["WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"],
      app_settings["WEBSITE_CONTENTOVERVNET"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["WEBSITE_DNS_ALT_SERVER"],
      app_settings["WEBSITE_DNS_SERVER"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"],
      app_settings["WEBSITE_RUN_FROM_PACKAGE"],
      app_settings["WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"],
      app_settings["WEBSITE_SWAP_WARMUP_PING_PATH"],
      app_settings["WEBSITE_SWAP_WARMUP_PING_STATUSES"],
      app_settings["WEBSITE_WARMUP_PATH"],
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"]
    ]
  }
}

module "azurerm_app_reg_fa_coordinator" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_resource_name}-coordinator-appreg"
  identifier_uris         = ["api://fa-${local.global_resource_name}-coordinator"]
  prevent_duplicate_names = true

  # use this code for adding api permissions
  required_resource_access = [{
    # Microsoft Graph
    resource_app_id = "00000003-0000-0000-c000-000000000000"
    resource_access = [{
      # User.Read
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
      type = "Scope"
    }]
  }]

  tags = ["terraform"]
}

module "azurerm_service_principal_fa_coordinator" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_coordinator.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_coordinator_pw" {
  service_principal_id = module.azurerm_service_principal_fa_coordinator.object_id
}

resource "azuread_application_password" "faap_fa_coordinator_app_service" {
  application_object_id = module.azurerm_app_reg_fa_coordinator.object_id
  end_date_relative     = "17520h"
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_coordinator_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_coordinator.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_coordinator_pe" {
  name                = "${azurerm_linux_function_app.fa_coordinator.name}-pe"
  resource_group_name = azurerm_resource_group.rg_coordinator.name
  location            = azurerm_resource_group.rg_coordinator.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_coordinator.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_coordinator.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Storage Account permissions
resource "azurerm_role_assignment" "ra_blob_delegator_coordinator" {
  scope                = azurerm_storage_account.sa.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_linux_function_app.fa_coordinator.identity[0].principal_id

  depends_on = [
    azurerm_storage_account.sa,
    azurerm_linux_function_app.fa_coordinator
  ]
}

resource "azurerm_role_assignment" "coordinator_blob_data_contributor" {
  scope                = azurerm_storage_container.container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_function_app.fa_coordinator.identity[0].principal_id

  depends_on = [
    azurerm_storage_account.sa,
    azurerm_storage_container.container,
    azurerm_linux_function_app.fa_coordinator
  ]
}