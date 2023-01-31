#################### Functions ####################

resource "azurerm_linux_function_app" "fa_text_extractor" {
  name                       = "fa-${local.resource_name}-text-extractor"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  service_plan_id            = azurerm_service_plan.asp-linux-ep.id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  virtual_network_subnet_id  = data.azurerm_subnet.polaris_textextractor_subnet.id
  functions_extension_version                  = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet"
    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = ""
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_VNET_ROUTE_ALL"                   = "1"
    "WEBSITE_DNS_SERVER"                       = "10.2.64.10"
    "WEBSITE_DNS_ALT_SERVER"                   = "10.3.64.10"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sa.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.pipeline_sa_text_extractor_file_share.name
    "AzureWebJobsStorage"                      = azurerm_storage_account.sa.primary_connection_string
    "BlobServiceContainerName"                 = azurerm_storage_container.container.name
    "BlobExpirySecs"                           = 3600
    "BlobUserDelegationKeyExpirySecs"          = 3600
    "BlobServiceUrl"                           = azurerm_storage_account.sa.primary_blob_endpoint
    "CallingAppTenantId"                       = data.azurerm_client_config.current.tenant_id
    "CallingAppValidAudience"                  = "api://fa-${local.resource_name}-text-extractor"
    "ComputerVisionClientServiceKey"           = azurerm_cognitive_account.computer_vision_service.primary_access_key
    "ComputerVisionClientServiceUrl"           = azurerm_cognitive_account.computer_vision_service.endpoint
    "SearchClientAuthorizationKey"             = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                  = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                    = jsondecode(file("search-index-definition.json")).name
  }
  https_only                 = true

  site_config {
    ip_restriction = []
    ftps_state     = "FtpsOnly"
    http2_enabled = true
    runtime_scale_monitoring_enabled = true
    vnet_route_all_enabled = true
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "RedirectToLoginPage"
    default_provider              = "AzureActiveDirectory"
    active_directory {
      client_id         = module.azurerm_app_reg_fa_text_extractor.client_id
      client_secret     = azuread_application_password.faap_fa_text_extractor_app_service.value
      allowed_audiences = ["api://fa-${local.resource_name}-text-extractor"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }
}

module "azurerm_app_reg_fa_text_extractor" {
  source  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name = "fa-${local.resource_name}-text-extractor-appreg"
  identifier_uris = ["api://fa-${local.resource_name}-text-extractor"]
  prevent_duplicate_names = true
  #use this code for adding app_roles
  app_role = [
    {
      allowed_member_types  = ["Application"]
      description          = "Can parse document texts using the ${local.resource_name} Polaris Text Extractor"
      display_name         = "Parse document texts in ${local.resource_name}"
      id                   = element(random_uuid.random_id[*].result, 3)
      value                = "application.extracttext"
    }
  ]
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
    redirect_uris = ["https://fa-${local.resource_name}-text-extractor.azurewebsites.net/.auth/login/aad/callback"]
    implicit_grant = {
      id_token_issuance_enabled     = true
    }
  }
  tags = ["fa-${local.resource_name}-text-extractor", "terraform"]
}

data "azurerm_function_app_host_keys" "ak_text_extractor" {
  name                = "fa-${local.resource_name}-text-extractor"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_linux_function_app.fa_text_extractor]
}

resource "azuread_application_password" "faap_fa_text_extractor_app_service" {
  application_object_id = module.azurerm_app_reg_fa_text_extractor.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_fa_text_extractor" {
  source         = "./modules/terraform-azurerm-azuread_service_principal"
  application_id = module.azurerm_app_reg_fa_text_extractor.client_id
  app_role_assignment_required = false
  owners         = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_text_extractor_pw" {
  service_principal_id = module.azurerm_service_principal_fa_text_extractor.object_id
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_text_extractor_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_text_extractor.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_text_extractor_pe" {
  name                  = "${azurerm_linux_function_app.fa_text_extractor.name}-pe"
  resource_group_name   = azurerm_resource_group.rg.name
  location              = azurerm_resource_group.rg.location
  subnet_id             = data.azurerm_subnet.polaris_apps_subnet.id

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_text_extractor.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_text_extractor.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "pipeline_text_extractor_dns_a" {
  name                = azurerm_linux_function_app.fa_text_extractor.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_text_extractor_pe.private_service_connection.0.private_ip_address]
}

# Create a second Private Endpoint to point to the SCM for deployments
resource "azurerm_private_endpoint" "pipeline_text_extractor_scm_pe" {
  name                  = "${azurerm_linux_function_app.fa_text_extractor.name}-scm-pe"
  resource_group_name   = azurerm_resource_group.rg.name
  location              = azurerm_resource_group.rg.location
  subnet_id             = data.azurerm_subnet.polaris_apps_subnet.id

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_text_extractor.name}-scm-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_text_extractor.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Create DNS A to match for SCM record
resource "azurerm_private_dns_a_record" "pipeline_text_extractor_scm_dns_a" {
  name                = "${azurerm_linux_function_app.fa_text_extractor.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_text_extractor_scm_pe.private_service_connection.0.private_ip_address]
}