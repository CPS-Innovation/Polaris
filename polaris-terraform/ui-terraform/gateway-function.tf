#################### Functions ####################
resource "azurerm_linux_function_app" "fa_polaris" {
  name                          = "fa-${local.resource_name}-gateway"
  location                      = azurerm_resource_group.rg_polaris.location
  resource_group_name           = azurerm_resource_group.rg_polaris.name
  service_plan_id               = azurerm_service_plan.asp_polaris_gateway.id
  storage_account_name          = azurerm_storage_account.sa_gateway.name
  storage_account_access_key    = azurerm_storage_account.sa_gateway.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_gateway_subnet.id
  functions_extension_version   = "~4"
  public_network_access_enabled = false
  https_only                    = true
  tags                          = local.common_tags
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_gateway.primary_connection_string
    "CallingAppValidAudience"                         = var.polaris_webapp_details.valid_audience
    "CallingAppValidRoles"                            = var.polaris_webapp_details.valid_roles
    "CallingAppValidScopes"                           = var.polaris_webapp_details.valid_scopes
    "ClientId"                                        = module.azurerm_app_reg_fa_polaris.client_id
    "ClientSecret"                                    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_polaris_client_secret.id})"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "HostType"                                        = "Production"
    "PolarisPipelineCoordinatorBaseUrl"               = "https://fa-${local.resource_name}-coordinator.azurewebsites.net/api/"
    "PolarisPdfThumbnailGeneratorBaseUrl"             = "https://fa-${local.resource_name}-pdf-thumbnail-generator.azurewebsites.net/api/"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.ui_logging.gateway_scale_controller
    "TenantId"                                        = data.azurerm_client_config.current.tenant_id
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_gateway.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.polaris_sa_gateway_file_share.name
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

  sticky_settings {
    app_setting_names = ["HostType"]
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
    pre_warmed_instance_count         = var.ui_component_service_plans.gateway_always_ready_instances
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
      app_settings["AzureWebJobsStorage"],
      app_settings["BlobContainerName"],
      app_settings["BlobExpirySecs"],
      app_settings["BlobServiceUrl"],
      app_settings["BlobUserDelegationKeyExpirySecs"],
      app_settings["CallingAppValidAudience"],
      app_settings["CallingAppValidRoles"],
      app_settings["CallingAppValidScopes"],
      app_settings["ClientId"],
      app_settings["ClientSecret"],
      app_settings["DdeiAccessKey"],
      app_settings["DdeiBaseUrl"],
      app_settings["HostType"],
      app_settings["PolarisPipelineCoordinatorBaseUrl"],
      app_settings["PolarisPdfThumbnailGeneratorBaseUrl"],
      app_settings["SCALE_CONTROLLER_LOGGING_ENABLED"],
      app_settings["TenantId"],
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

  depends_on = [azurerm_storage_account.sa_gateway, azapi_resource.polaris_sa_gateway_file_share]
}

module "azurerm_app_reg_fa_polaris" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.resource_name}-gateway-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-gateway"]
  owners                  = [data.azuread_client_config.current.object_id]
  prevent_duplicate_names = true
  group_membership_claims = ["ApplicationGroup"]
  optional_claims = {
    access_token = {
      name = "groups"
    }
    id_token = {
      name = "groups"
    }
    saml2_token = {
      name = "groups"
    }
  }
  #use this code for adding scopes
  api = {
    mapped_claims_enabled          = true
    requested_access_token_version = 1
    known_client_applications      = []
    oauth2_permission_scope = [{
      admin_consent_description  = "Allow the calling application to make requests of the ${local.resource_name} Gateway"
      admin_consent_display_name = "Call the ${local.resource_name} Gateway"
      id                         = element(random_uuid.random_id[*].result, 0)
      type                       = "Admin"
      user_consent_description   = "Interact with the ${local.resource_name} Gateway on-behalf of the calling user"
      user_consent_display_name  = "Interact with the ${local.resource_name} Gateway"
      value                      = "user_impersonation"
    }]
  }
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
  web = {
    redirect_uris = ["https://fa-${local.resource_name}-gateway.azurewebsites.net/.auth/login/aad/callback"]
    implicit_grant = {
      id_token_issuance_enabled     = true
      access_token_issuance_enabled = true
    }
  }
  tags = ["terraform"]
}

resource "azuread_application_password" "faap_polaris_app_service" {
  application_object_id = module.azurerm_app_reg_fa_polaris.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_sp_polaris_gateway" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_polaris.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_polaris_gateway_pw" {
  service_principal_id = module.azurerm_service_principal_sp_polaris_gateway.object_id
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_gateway_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}