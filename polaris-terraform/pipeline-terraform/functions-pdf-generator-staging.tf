#################### Functions #####################
#################### Staging1 ######################
resource "azurerm_windows_function_app_slot" "fa_pdf_generator_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_windows_function_app.fa_pdf_generator.id
  storage_account_name          = azurerm_storage_account.sa_pdf_generator.name
  storage_account_access_key    = azurerm_storage_account.sa_pdf_generator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_pdfgenerator_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_pdf_generator_file_share_staging1.name
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.pipeline_logging.pdf_generator_scale_controller
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                        = "documents"
    "HteFeatureFlag"                                  = var.hte_feature_flag
    "ImageConversion__Resolution"                     = var.image_conversion_redaction.resolution
    "ImageConversion__QualityPercent"                 = var.image_conversion_redaction.quality_percent
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
      dotnet_version = "v6.0"
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
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

module "azurerm_app_reg_fa_pdf_generator_staging1" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_name}-pdf-generator-staging1-appreg"
  identifier_uris         = ["api://fa-${local.global_name}-pdf-generator-staging1"]
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

resource "azuread_application_password" "faap_fa_pdf_generator_staging1_app_service" {
  application_object_id = module.azurerm_app_reg_fa_pdf_generator_staging1.object_id
  end_date_relative     = "17520h"
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_generator_staging1_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging1-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_generator.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}

#################### Staging2 ######################
resource "azurerm_windows_function_app_slot" "fa_pdf_generator_staging2" {
  name                          = "staging2"
  function_app_id               = azurerm_windows_function_app.fa_pdf_generator.id
  storage_account_name          = azurerm_storage_account.sa_pdf_generator.name
  storage_account_access_key    = azurerm_storage_account.sa_pdf_generator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_pdfgenerator_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_pdf_generator_file_share_staging2.name
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.pipeline_logging.pdf_generator_scale_controller
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                        = "documents"
    "HteFeatureFlag"                                  = var.hte_feature_flag
    "ImageConversion__Resolution"                     = var.image_conversion_redaction.resolution
    "ImageConversion__QualityPercent"                 = var.image_conversion_redaction.quality_percent
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
      dotnet_version = "v6.0"
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
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

module "azurerm_app_reg_fa_pdf_generator_staging2" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_name}-pdf-generator-staging2-appreg"
  identifier_uris         = ["api://fa-${local.global_name}-pdf-generator-staging2"]
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

resource "azuread_application_password" "faap_fa_pdf_generator_staging2_app_service" {
  application_object_id = module.azurerm_app_reg_fa_pdf_generator_staging2.object_id
  end_date_relative     = "17520h"
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_generator_staging2_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging2-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging2-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_generator.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging2"]
  }
}