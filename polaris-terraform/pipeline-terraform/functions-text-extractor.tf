#################### Functions ####################

resource "azurerm_linux_function_app" "fa_text_extractor" {
  name                          = "fa-${local.global_name}-text-extractor"
  location                      = azurerm_resource_group.rg.location
  resource_group_name           = azurerm_resource_group.rg.name
  service_plan_id               = azurerm_service_plan.asp_polaris_ep_text_extractor.id
  storage_account_name          = azurerm_storage_account.sa_text_extractor.name
  storage_account_access_key    = azurerm_storage_account.sa_text_extractor.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_textextractor_2_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  builtin_logging_enabled       = false

  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_text_extractor.primary_connection_string
    "ComputerVisionClientServiceKey"                  = azurerm_cognitive_account.computer_vision_service.primary_access_key
    "ComputerVisionClientServiceUrl"                  = azurerm_cognitive_account.computer_vision_service.endpoint
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "HostType"                                        = "Production"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.pipeline_logging.text_extractor_scale_controller
    "SearchClientAuthorizationKey"                    = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                         = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                           = jsondecode(file("search-index-definition.json")).name
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_text_extractor.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_text_extractor_file_share.name
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
  }

  sticky_settings {
    app_setting_names = ["HostType"]
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    runtime_scale_monitoring_enabled       = true
    vnet_route_all_enabled                 = true
    elastic_instance_minimum               = var.pipeline_component_service_plans.text_extractor_always_ready_instances
    app_scale_limit                        = var.pipeline_component_service_plans.text_extractor_maximum_scale_out_limit
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    application_stack {
      dotnet_version = "6.0"
    }
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
  }

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
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }
}

module "azurerm_app_reg_fa_text_extractor" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_name}-text-extractor-appreg"
  identifier_uris         = ["api://fa-${local.global_name}-text-extractor"]
  prevent_duplicate_names = true
  #use this code for adding app_roles
  /*app_role = [
    {
      allowed_member_types = ["Application"]
      description          = "Can parse document texts using the ${local.resource_name} Polaris Text Extractor"
      display_name         = "Parse document texts in ${local.resource_name}"
      id                   = element(random_uuid.random_id[*].result, 3)
      value                = "application.extracttext"
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

resource "azuread_application_password" "faap_fa_text_extractor_app_service" {
  application_object_id = module.azurerm_app_reg_fa_text_extractor.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_fa_text_extractor" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_text_extractor.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
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
  name                = "${azurerm_linux_function_app.fa_text_extractor.name}-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_text_extractor.name}-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_text_extractor.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}