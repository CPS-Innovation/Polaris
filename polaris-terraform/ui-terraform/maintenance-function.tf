#################### Functions ####################
resource "azurerm_linux_function_app" "fa_polaris_maintenance" {
  name                          = "fa-${local.resource_name}-maintenance"
  location                      = azurerm_resource_group.rg_polaris.location
  resource_group_name           = azurerm_resource_group.rg_polaris.name
  service_plan_id               = azurerm_service_plan.asp_polaris_maintenance.id
  storage_account_name          = azurerm_storage_account.sa_gateway.name
  storage_account_access_key    = azurerm_storage_account.sa_gateway.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_maintenance_subnet.id
  functions_extension_version   = "~4"
  public_network_access_enabled = false
  https_only                    = true
  tags                          = local.common_tags
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobsStorage"                      = azurerm_storage_account.sa_gateway.primary_connection_string
    "FUNCTIONS_EXTENSION_VERSION"              = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet-isolated"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sa_gateway.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.polaris_sa_maintenance_file_share.name
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = "1"
    "WEBSITE_RUN_FROM_PACKAGE"                 = "1"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = "true"
  }

  site_config {
    always_on                         = true
    ftps_state                        = "FtpsOnly"
    http2_enabled                     = true
    vnet_route_all_enabled            = true
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
    use_32_bit_worker                 = false
    application_stack {
      dotnet_version = "8.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = false
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "AllowAnonymous"
  }

  depends_on = [azurerm_storage_account.sa_gateway, azapi_resource.polaris_sa_maintenance_file_share]
}

module "azurerm_app_reg_fa_maintenance" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.resource_name}-maintenance-appreg"
  identifier_uris         = ["api://fa-${local.resource_name}-maintenance"]
  prevent_duplicate_names = true

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

resource "azuread_application_password" "faap_fa_maintenance_app_service" {
  application_object_id = module.azurerm_app_reg_fa_maintenance.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_fa_maintenance" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_maintenance.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_maintenance_pw" {
  service_principal_id = module.azurerm_service_principal_fa_maintenance.object_id
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_maintenance_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_maintenance.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_maintenance_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris_maintenance.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris_maintenance.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris_maintenance.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}