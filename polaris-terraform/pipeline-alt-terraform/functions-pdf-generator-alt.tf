#################### Functions ####################

resource "azurerm_windows_function_app" "fa_pdf_generator_alt" {
  name                        = "fa-${local.alt_resource_name}-pdf-generator"
  location                    = data.azurerm_resource_group.rg_polaris_pipeline.location
  resource_group_name         = data.azurerm_resource_group.rg_polaris_pipeline.name
  service_plan_id             = data.azurerm_service_plan.pipeline_service_plan.id
  storage_account_name        = data.azurerm_storage_account.pipeline_sa.name
  storage_account_access_key  = data.azurerm_storage_account.pipeline_sa.primary_access_key
  virtual_network_subnet_id   = data.azurerm_subnet.polaris_pdfgenerator_subnet.id
  tags                        = local.common_tags
  functions_extension_version = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"              = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = "true"
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = data.azurerm_storage_account.pipeline_sa.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.pipeline_sa_pdf_generator_alt_file_share.name
    "SCALE_CONTROLLER_LOGGING_ENABLED"         = var.pipeline_logging.pdf_generator_scale_controller
    "AzureWebJobsStorage"                      = data.azurerm_storage_account.pipeline_sa.primary_connection_string
    "BlobServiceUrl"                           = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                 = "documents"
    "BlobServiceConnectionString"              = "@Microsoft.KeyVault(SecretUri=${data.azurerm_key_vault_secret.pipeline_storage_connection_string.id})"
    "SearchClientAuthorizationKey"             = data.azurerm_search_service.pipeline_search_service.primary_key
    "SearchClientEndpointUrl"                  = "https://${data.azurerm_search_service.pipeline_search_service.name}.search.windows.net"
    "SearchClientIndexName"                    = jsondecode(file("search-index-definition.json")).name
  }
  https_only = true

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    runtime_scale_monitoring_enabled       = true
    vnet_route_all_enabled                 = true
    elastic_instance_minimum               = 3 
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
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

module "azurerm_app_reg_fa_pdf_generator_alt" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.alt_resource_name}-pdf-generator-appreg"
  identifier_uris         = ["api://fa-${local.alt_resource_name}-pdf-generator"]
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

data "azurerm_function_app_host_keys" "ak_pdf_generator_alt" {
  name                = "fa-${local.alt_resource_name}-pdf-generator"
  resource_group_name = data.azurerm_resource_group.rg_polaris_pipeline.name
  depends_on          = [azurerm_windows_function_app.fa_pdf_generator_alt]
}

module "azurerm_service_principal_fa_pdf_generator_alt" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_pdf_generator_alt.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_pdf_generator_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_pdf_generator_alt.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_generator_alt_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_generator_alt.name}-pe"
  resource_group_name = data.azurerm_resource_group.rg_polaris_pipeline.name
  location            = data.azurerm_resource_group.rg_polaris_pipeline.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_generator_alt.name}-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_generator_alt.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "pipeline_pdf_generator_alt_dns_a" {
  name                = azurerm_windows_function_app.fa_pdf_generator_alt.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_pdf_generator_alt_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}

# Create DNS A to match for SCM record for SCM deployments
resource "azurerm_private_dns_a_record" "pipeline_pdf_generator_alt_scm_dns_a" {
  name                = "${azurerm_windows_function_app.fa_pdf_generator_alt.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_pdf_generator_alt_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}