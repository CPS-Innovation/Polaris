#################### Functions ####################

resource "azurerm_windows_function_app" "fa_pdf_generator" {
  name                          = "fa-${local.global_name}-pdf-generator"
  location                      = azurerm_resource_group.rg.location
  resource_group_name           = azurerm_resource_group.rg.name
  service_plan_id               = azurerm_service_plan.asp_polaris_pipeline_ep_pdf_generator.id
  storage_account_name          = azurerm_storage_account.sa_pdf_generator.name
  storage_account_access_key    = azurerm_storage_account.sa_pdf_generator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_pdfgenerator_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                 = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"              = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"      = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                 = "1"
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.pipeline_sa_pdf_generator_file_share.name
    "SCALE_CONTROLLER_LOGGING_ENABLED"         = var.pipeline_logging.pdf_generator_scale_controller
    "AzureWebJobsStorage"                      = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "BlobServiceUrl"                           = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                 = "documents"
    "HteFeatureFlag"                           = var.hte_feature_flag
    "ImageConversion__Resolution"              = var.image_conversion_redaction.resolution
    "ImageConversion__QualityPercent"          = var.image_conversion_redaction.quality_percent
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    runtime_scale_monitoring_enabled       = true
    vnet_route_all_enabled                 = true
    elastic_instance_minimum               = var.pipeline_component_service_plans.pdf_generator_always_ready_instances
    app_scale_limit                        = var.pipeline_component_service_plans.pdf_generator_maximum_scale_out_limit
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    application_stack {
      dotnet_version = "6.0"
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
}

module "azurerm_app_reg_fa_pdf_generator" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_name}-pdf-generator-appreg"
  identifier_uris         = ["api://fa-${local.global_name}-pdf-generator"]
  prevent_duplicate_names = true
  #use this code for adding app_roles
  /*app_role = [
    {
      allowed_member_types = ["Application"]
      description          = "Can create PDF resources using the ${local.resource_name} PDF Generator"
      display_name         = "Create PDF resources"
      id                   = element(random_uuid.random_id[*].result, 2)
      value                = "application.create"
    }
  ]*/
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

data "azurerm_function_app_host_keys" "ak_pdf_generator" {
  name                = "fa-${local.global_name}-pdf-generator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on          = [azurerm_windows_function_app.fa_pdf_generator]
}

resource "azuread_application_password" "faap_fa_pdf_generator_app_service" {
  application_object_id = module.azurerm_app_reg_fa_pdf_generator.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_fa_pdf_generator" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_pdf_generator.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_pdf_generator_pw" {
  service_principal_id = module.azurerm_service_principal_fa_pdf_generator.object_id
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_pdf_generator_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_pdf_generator.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_generator_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_generator.name}-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_generator.name}-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_generator.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}