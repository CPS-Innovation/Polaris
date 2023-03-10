#################### Functions ####################

resource "azurerm_linux_function_app" "fa_coordinator" {
  name                        = "fa-${local.resource_name}-coordinator"
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  service_plan_id             = azurerm_service_plan.asp-linux-ep.id
  storage_account_name        = azurerm_storage_account.sa.name
  storage_account_access_key  = azurerm_storage_account.sa.primary_access_key
  virtual_network_subnet_id   = data.azurerm_subnet.polaris_coordinator_subnet.id
  tags                        = local.common_tags
  functions_extension_version = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"              = "~4"
    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = "true"
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sa.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.pipeline_sa_coordinator_file_share.name
    "AzureWebJobsStorage"                      = azurerm_storage_account.sa.primary_connection_string
    "CoordinatorOrchestratorTimeoutSecs"       = "600"
    "PdfGeneratorUrl"                          = "https://fa-${local.resource_name}-pdf-generator.azurewebsites.net/api/generate?code=${data.azurerm_function_app_host_keys.ak_pdf_generator.default_function_key}"
    "TextExtractorUrl"                         = "https://fa-${local.resource_name}-text-extractor.azurewebsites.net/api/extract?code=${data.azurerm_function_app_host_keys.ak_text_extractor.default_function_key}"
    "DocumentsRepositoryBaseUrl"               = "https://fa-${local.ddei_resource_name}.azurewebsites.net/api/"
    "ListDocumentsUrl"                         = "urns/{0}/cases/{1}/documents?code=${data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key}"
    "SearchClientAuthorizationKey"             = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                  = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                    = jsondecode(file("search-index-definition.json")).name
    "BlobServiceUrl"                           = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                        = "documents"
    "BlobExpirySecs"                           = 3600
    "BlobUserDelegationKeyExpirySecs"          = 3600
  }
  https_only = true

  site_config {
    ip_restriction                   = []
    ftps_state                       = "FtpsOnly"
    http2_enabled                    = true
    runtime_scale_monitoring_enabled = true
    vnet_route_all_enabled           = true

    cors {
      allowed_origins = []
    }
    elastic_instance_minimum = 1
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = false
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "AllowAnonymous"
  }
}

data "azurerm_function_app_host_keys" "ak_coordinator" {
  name                = "fa-${local.resource_name}-coordinator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on          = [azurerm_linux_function_app.fa_coordinator]
}

module "azurerm_app_reg_fa_coordinator" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.resource_name}-coordinator-appreg"
  identifier_uris         = ["api://fa-${local.resource_name}-coordinator"]
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
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
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

# Create DNS A Record
resource "azurerm_private_dns_a_record" "pipeline_coordinator_dns_a" {
  name                = azurerm_linux_function_app.fa_coordinator.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_coordinator_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}

# Create DNS A to match for SCM record for SCM deployments
resource "azurerm_private_dns_a_record" "pipeline_coordinator_scm_dns_a" {
  name                = "${azurerm_linux_function_app.fa_coordinator.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_coordinator_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}