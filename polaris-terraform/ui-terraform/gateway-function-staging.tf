resource "azurerm_linux_function_app_slot" "fa_polaris_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_linux_function_app.fa_polaris.id
  storage_account_name          = azurerm_storage_account.sacpspolaris.name
  storage_account_access_key    = azurerm_storage_account.sacpspolaris.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_gateway_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sacpspolaris.primary_connection_string
    "BlobContainerName"                               = "documents"
    "BlobExpirySecs"                                  = 3600
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobUserDelegationKeyExpirySecs"                 = 3600
    "CallingAppValidAudience"                         = var.polaris_webapp_details.valid_audience
    "CallingAppValidRoles"                            = var.polaris_webapp_details.valid_roles
    "CallingAppValidScopes"                           = var.polaris_webapp_details.valid_scopes
    "ClientId"                                        = module.azurerm_app_reg_fa_polaris.client_id
    "ClientSecret"                                    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_polaris_client_secret.id})"
    "DdeiAccessKey"                                   = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key
    "DdeiBaseUrl"                                     = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "HostType"                                        = "Staging1"
    "PolarisPipelineCoordinatorBaseUrl"               = "https://fa-${local.resource_name}-coordinator.azurewebsites.net/api/"
    "PolarisPipelineCoordinatorDurableExtensionCode"  = data.azurerm_function_app_host_keys.fa_coordinator_host_keys.durabletask_extension_key
    "PolarisPipelineCoordinatorFunctionAppKey"        = data.azurerm_function_app_host_keys.fa_coordinator_host_keys.default_function_key
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.ui_logging.gateway_scale_controller
    "TenantId"                                        = data.azurerm_client_config.current.tenant_id
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.polaris_sacpspolaris_gateway_staging1_file_share.name
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
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

  site_config {
    always_on                              = false
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    cors {
      allowed_origins = [
        "https://as-web-${local.resource_name}.azurewebsites.net",
        "https://${local.resource_name}-cmsproxy.azurewebsites.net",
        "https://${local.resource_name}-notprod.cps.gov.uk",
        var.env == "dev" ? "http://localhost:3000" : ""
      ]
      support_credentials = true
    }
    vnet_route_all_enabled            = true
    runtime_scale_monitoring_enabled  = true
    elastic_instance_minimum          = var.ui_component_service_plans.gateway_always_ready_instances
    app_scale_limit                   = var.ui_component_service_plans.gateway_maximum_scale_out_limit
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
    application_stack {
      dotnet_version = "6.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    default_provider       = "AzureActiveDirectory"
    unauthenticated_action = "RedirectToLoginPage"
    excluded_paths         = ["/api/status"]

    # our default_provider:
    active_directory_v2 {
      tenant_auth_endpoint = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      #checkov:skip=CKV_SECRET_6:Base64 High Entropy String - Misunderstanding of setting "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = module.azurerm_app_reg_fa_polaris.client_id
      allowed_audiences          = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-gateway"]
    }

    login {
      token_store_enabled = false
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_gateway_staging1_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris.name}-staging1-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}