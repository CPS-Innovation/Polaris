#################### Staging1 ######################
resource "azurerm_linux_function_app_slot" "fa_polaris_auth_handover_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_linux_function_app.fa_polaris_auth_handover.id
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_auth_handover_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  public_network_access_enabled = false
  https_only                    = true

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"              = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = ""
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.polaris_sacpspolaris_auth_handover_staging1_file_share.name
    "SCALE_CONTROLLER_LOGGING_ENABLED"         = var.ui_logging.auth_handover_scale_controller
    "AzureWebJobsStorage"                      = azurerm_storage_account.sacpspolaris.primary_connection_string
    "DdeiBaseUrl"                              = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                            = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key
  }

  site_config {
    always_on                              = true
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    application_stack {
      dotnet_version = "6.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings_v2 {
    auth_enabled           = false
    unauthenticated_action = "AllowAnonymous"
    default_provider       = "AzureActiveDirectory"
    excluded_paths         = ["/status"]

    active_directory_v2 {
      tenant_auth_endpoint = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      #checkov:skip=CKV_SECRET_6:Base64 High Entropy String - Misunderstanding of setting "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = module.azurerm_app_reg_fa_polaris_auth_handover_staging1.client_id
    }

    login {
      token_store_enabled = false
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }

  depends_on = [azurerm_storage_account.sacpspolaris, azapi_resource.polaris_sacpspolaris_auth_handover_staging1_file_share]
}

module "azurerm_app_reg_fa_polaris_auth_handover_staging1" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.resource_name}-auth-handover-staging1-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-auth-handover-staging1"]
  owners                  = [data.azuread_client_config.current.object_id]
  prevent_duplicate_names = true
  #use this code for adding api permissions
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

resource "azuread_application_password" "faap_polaris_auth_handover_service_staging1" {
  application_object_id = module.azurerm_app_reg_fa_polaris_auth_handover_staging1.object_id
  end_date_relative     = "17520h"
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_auth_handover_staging1_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}-staging1-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris_auth_handover.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}

#################### Staging2 ######################
resource "azurerm_linux_function_app_slot" "fa_polaris_auth_handover_staging2" {
  name                          = "staging1"
  function_app_id               = azurerm_linux_function_app.fa_polaris_auth_handover.id
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_auth_handover_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  public_network_access_enabled = false
  https_only                    = true

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"              = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = ""
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.polaris_sacpspolaris_auth_handover_staging2_file_share.name
    "SCALE_CONTROLLER_LOGGING_ENABLED"         = var.ui_logging.auth_handover_scale_controller
    "AzureWebJobsStorage"                      = azurerm_storage_account.sacpspolaris.primary_connection_string
    "DdeiBaseUrl"                              = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                            = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key
  }

  site_config {
    always_on                              = true
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    application_stack {
      dotnet_version = "6.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings_v2 {
    auth_enabled           = false
    unauthenticated_action = "AllowAnonymous"
    default_provider       = "AzureActiveDirectory"
    excluded_paths         = ["/status"]

    active_directory_v2 {
      tenant_auth_endpoint = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      #checkov:skip=CKV_SECRET_6:Base64 High Entropy String - Misunderstanding of setting "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = module.azurerm_app_reg_fa_polaris_auth_handover_staging2.client_id
    }

    login {
      token_store_enabled = false
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }

  depends_on = [azurerm_storage_account.sacpspolaris, azapi_resource.polaris_sacpspolaris_auth_handover_staging2_file_share]
}

module "azurerm_app_reg_fa_polaris_auth_handover_staging2" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.resource_name}-auth-handover-staging2-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-auth-handover-staging2"]
  owners                  = [data.azuread_client_config.current.object_id]
  prevent_duplicate_names = true
  #use this code for adding api permissions
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

resource "azuread_application_password" "faap_polaris_auth_handover_service_staging2" {
  application_object_id = module.azurerm_app_reg_fa_polaris_auth_handover_staging2.object_id
  end_date_relative     = "17520h"
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_auth_handover_staging2_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}-staging2-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}-staging2-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris_auth_handover.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging2"]
  }
}