#################### Functions ####################
resource "azurerm_linux_function_app" "fa_polaris" {
  name                        = "fa-${local.resource_name}-gateway"
  location                    = azurerm_resource_group.rg_polaris.location
  resource_group_name         = azurerm_resource_group.rg_polaris.name
  service_plan_id             = azurerm_service_plan.asp_polaris_gateway.id
  storage_account_name        = azurerm_storage_account.sacpspolaris.name
  storage_account_access_key  = azurerm_storage_account.sacpspolaris.primary_access_key
  virtual_network_subnet_id   = data.azurerm_subnet.polaris_gateway_subnet.id
  functions_extension_version = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "APPINSIGHTS_INSTRUMENTATIONKEY"                  = data.azurerm_application_insights.global_ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.polaris_sacpspolaris_gateway_file_share.name
    "AzureWebJobsStorage"                             = azurerm_storage_account.sacpspolaris.primary_connection_string
    "TenantId"                                        = data.azurerm_client_config.current.tenant_id
    "ClientId"                                        = module.azurerm_app_reg_fa_polaris.client_id
    "ClientSecret"                                    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_polaris_client_secret.id})"
    "PolarisPipelineCoordinatorBaseUrl"               = "https://fa-${local.pipeline_resource_name}-coordinator.azurewebsites.net/api/"
    "PolarisPipelineCoordinatorFunctionAppKey"        = data.azurerm_function_app_host_keys.fa_pipeline_coordinator_host_keys.default_function_key
    "PolarisPipelineCoordinatorDurableExtensionCode"  = data.azurerm_function_app_host_keys.fa_pipeline_coordinator_host_keys.durabletask_extension_key
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobContainerName"                               = "documents"
    "BlobServiceConnectionString"                     = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_ui_storage_connection_string.id})"
    "BlobExpirySecs"                                  = 3600
    "BlobUserDelegationKeyExpirySecs"                 = 3600
    "CallingAppValidAudience"                         = var.polaris_webapp_details.valid_audience
    "CallingAppValidScopes"                           = var.polaris_webapp_details.valid_scopes
    "CallingAppValidRoles"                            = var.polaris_webapp_details.valid_roles
    "DdeiBaseUrl"                                     = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                                   = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key
  }

  site_config {
    always_on      = false
    ftps_state     = "FtpsOnly"
    http2_enabled  = true
    cors {
      allowed_origins = [
        "https://as-web-${local.resource_name}.azurewebsites.net",
        "https://${local.resource_name}-cmsproxy.azurewebsites.net",
        "https://${local.resource_name}-notprod.cpsdev.co.uk",
        var.env == "dev" ? "http://localhost:3000" : ""
      ]
      support_credentials = true
    }
    vnet_route_all_enabled           = true
    runtime_scale_monitoring_enabled = true
  }

  tags = local.common_tags

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "RedirectToLoginPage"
    default_provider              = "AzureActiveDirectory"
    active_directory {
      client_id         = module.azurerm_app_reg_fa_polaris.client_id
      client_secret     = azuread_application_password.asap_web_polaris_app_service.value
      allowed_audiences = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-gateway"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"]
    ]
  }

  depends_on = [azurerm_storage_account.sacpspolaris, azapi_resource.polaris_sacpspolaris_gateway_file_share]
}

module "azurerm_app_reg_fa_polaris" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.resource_name}-gateway-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-gateway"]
  owners                  = [data.azuread_client_config.current.object_id]
  prevent_duplicate_names = true
  #use this code for adding scopes
  api = {
    mapped_claims_enabled          = false
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

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_gateway_dns_a" {
  name                = azurerm_linux_function_app.fa_polaris.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_gateway_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_gateway_pe]
}

# Create DNS A Record for SCM site
resource "azurerm_private_dns_a_record" "polaris_gateway_scm_dns_a" {
  name                = "${azurerm_linux_function_app.fa_polaris.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_gateway_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_gateway_pe]
}