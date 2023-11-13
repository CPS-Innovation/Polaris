#################### Functions #####################
#################### Staging1 ######################
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

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                       = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                    = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"            = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                       = "1"
    "WEBSITE_CONTENTOVERVNET"                        = "1"
    "WEBSITE_DNS_SERVER"                             = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                         = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"       = azurerm_storage_account.sa_coordinator.primary_connection_string
    "WEBSITE_CONTENTSHARE"                           = azapi_resource.pipeline_sa_coordinator_file_share_staging1.name
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"   = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"     = "0"
    "SCALE_CONTROLLER_LOGGING_ENABLED"               = var.pipeline_logging.coordinator_scale_controller
    "AzureWebJobsStorage"                            = azurerm_storage_account.sa_coordinator.primary_connection_string
    "CoordinatorOrchestratorTimeoutSecs"             = "600"
    "PolarisPipelineCoordinatorBaseUrl"              = "https://fa-${local.global_name}-coordinator.azurewebsites.net/api/"
    "PolarisPipelineCoordinatorDurableExtensionCode" = "" //set in deployment script
    "PolarisPipelineTextExtractorBaseUrl"            = "https://fa-${local.global_name}-text-extractor.azurewebsites.net/api/"
    "PolarisPipelineTextExtractorFunctionAppKey"     = "" //set in deployment script
    "SearchClientAuthorizationKey"                   = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                        = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                          = jsondecode(file("search-index-definition.json")).name
    "BlobServiceUrl"                                 = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                       = "documents"
    "BlobExpirySecs"                                 = 3600
    "BlobUserDelegationKeyExpirySecs"                = 3600
    "DdeiBaseUrl"                                    = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                                  = "" //set in deployment script
    "PolarisPipelineRedactPdfBaseUrl"                = "https://fa-${local.global_name}-pdf-generator.azurewebsites.net/api/"
    "PolarisPipelineRedactPdfFunctionAppKey"         = "" //set in deployment script
    "OvernightClearDownEnabled"                      = var.overnight_clear_down_enabled
    "SlidingClearDownEnabled"                        = var.sliding_clear_down_enabled
    "SlidingClearDownInputDays"                      = var.sliding_clear_down_input_days
    "CoordinatorTaskHub"                             = "fapolaris${var.env != "prod" ? var.env : ""}coordinatorstaging1"
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
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["PolarisPipelineCoordinatorDurableExtensionCode"],
      app_settings["PolarisPipelineTextExtractorFunctionAppKey"],
      app_settings["DdeiAccessKey"],
      app_settings["PolarisPipelineRedactPdfFunctionAppKey"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

module "azurerm_app_reg_fa_coordinator_staging1" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_name}-coordinator-staging1-appreg"
  identifier_uris         = ["api://fa-${local.global_name}-coordinator-staging1"]
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

resource "azuread_application_password" "faap_fa_coordinator_staging1_app_service" {
  application_object_id = module.azurerm_app_reg_fa_coordinator_staging1.object_id
  end_date_relative     = "17520h"
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

#################### Staging2 ######################
resource "azurerm_linux_function_app_slot" "fa_coordinator_staging2" {
  name                          = "staging2"
  function_app_id               = azurerm_linux_function_app.fa_coordinator.id
  storage_account_name          = azurerm_storage_account.sa_coordinator.name
  storage_account_access_key    = azurerm_storage_account.sa_coordinator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_coordinator_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                       = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                    = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"            = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                       = "1"
    "WEBSITE_CONTENTOVERVNET"                        = "1"
    "WEBSITE_DNS_SERVER"                             = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                         = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"       = azurerm_storage_account.sa_coordinator.primary_connection_string
    "WEBSITE_CONTENTSHARE"                           = azapi_resource.pipeline_sa_coordinator_file_share_staging2.name
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"   = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"     = "0"
    "SCALE_CONTROLLER_LOGGING_ENABLED"               = var.pipeline_logging.coordinator_scale_controller
    "AzureWebJobsStorage"                            = azurerm_storage_account.sa_coordinator.primary_connection_string
    "CoordinatorOrchestratorTimeoutSecs"             = "600"
    "PolarisPipelineCoordinatorBaseUrl"              = "https://fa-${local.global_name}-coordinator.azurewebsites.net/api/"
    "PolarisPipelineCoordinatorDurableExtensionCode" = "" //set in deployment script
    "PolarisPipelineTextExtractorBaseUrl"            = "https://fa-${local.global_name}-text-extractor.azurewebsites.net/api/"
    "PolarisPipelineTextExtractorFunctionAppKey"     = "" //set in deployment script
    "SearchClientAuthorizationKey"                   = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                        = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                          = jsondecode(file("search-index-definition.json")).name
    "BlobServiceUrl"                                 = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                       = "documents"
    "BlobExpirySecs"                                 = 3600
    "BlobUserDelegationKeyExpirySecs"                = 3600
    "DdeiBaseUrl"                                    = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                                  = "" //set in deployment script
    "PolarisPipelineRedactPdfBaseUrl"                = "https://fa-${local.global_name}-pdf-generator.azurewebsites.net/api/"
    "PolarisPipelineRedactPdfFunctionAppKey"         = "" //set in deployment script
    "OvernightClearDownEnabled"                      = var.overnight_clear_down_enabled
    "SlidingClearDownEnabled"                        = var.sliding_clear_down_enabled
    "SlidingClearDownInputDays"                      = var.sliding_clear_down_input_days
    "CoordinatorTaskHub"                             = "fapolaris${var.env != "prod" ? var.env : ""}coordinatorstaging2"
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
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["PolarisPipelineCoordinatorDurableExtensionCode"],
      app_settings["PolarisPipelineTextExtractorFunctionAppKey"],
      app_settings["DdeiAccessKey"],
      app_settings["PolarisPipelineRedactPdfFunctionAppKey"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

module "azurerm_app_reg_fa_coordinator_staging2" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_name}-coordinator-staging2-appreg"
  identifier_uris         = ["api://fa-${local.global_name}-coordinator-staging2"]
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

resource "azuread_application_password" "faap_fa_coordinator_staging2_app_service" {
  application_object_id = module.azurerm_app_reg_fa_coordinator_staging2.object_id
  end_date_relative     = "17520h"
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_coordinator_staging2_pe" {
  name                = "${azurerm_linux_function_app.fa_coordinator.name}-staging2-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_coordinator.name}-staging2-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_coordinator.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging2"]
  }
}