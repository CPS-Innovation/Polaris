resource "azurerm_linux_function_app" "fa_polaris_auth_handover" {
  name                        = "fa-${local.resource_name}-auth-handover"
  location                    = azurerm_resource_group.rg_polaris.location
  resource_group_name         = azurerm_resource_group.rg_polaris.name
  service_plan_id             = azurerm_service_plan.asp_polaris.id
  storage_account_name        = azurerm_storage_account.sacpspolaris.name
  storage_account_access_key  = azurerm_storage_account.sacpspolaris.primary_access_key
  virtual_network_subnet_id   = data.azurerm_subnet.polaris_auth_handover_subnet.id
  functions_extension_version = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"              = "~4"
    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.ai_polaris.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = ""
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = "10.7.197.20"
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.polaris_sacpspolaris_auth_handover_file_share.name
    "AzureWebJobsStorage"                      = azurerm_storage_account.sacpspolaris.primary_connection_string
  }

  site_config {
    ftps_state             = "FtpsOnly"
    http2_enabled          = true
    ip_restriction         = []
    vnet_route_all_enabled = true
  }

  tags = local.common_tags

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
    ]
  }

  depends_on = [azurerm_storage_account.sacpspolaris, azapi_resource.polaris_sacpspolaris_gateway_file_share]
}

module "azurerm_app_reg_fa_polaris_auth_handover" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.resource_name}-auth-handover-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-auth-handover"]
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

resource "azuread_application_password" "faap_polaris_auth_handover_service" {
  application_object_id = module.azurerm_app_reg_fa_polaris_auth_handover.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_sp_polaris_auth_handover" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_polaris_auth_handover.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_polaris_auth_handover_pw" {
  service_principal_id = module.azurerm_service_principal_sp_polaris_auth_handover.object_id
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_auth_handover_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris_auth_handover.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_auth_handover_dns_a" {
  name                = azurerm_linux_function_app.fa_polaris_auth_handover.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_auth_handover_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_auth_handover_pe]
}

# Create DNS A Record for SCM site
resource "azurerm_private_dns_a_record" "polaris_auth_handover_scm_dns_a" {
  name                = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_auth_handover_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_auth_handover_pe]
}