#################### Functions ####################

resource "azurerm_windows_function_app" "fa_pdf_thumbnail_generator" {
  name                          = "fa-${local.global_resource_name}-pdf-thumb-gen"
  location                      = azurerm_resource_group.rg_coordinator.location
  resource_group_name           = azurerm_resource_group.rg_coordinator.name
  service_plan_id               = azurerm_service_plan.asp_polaris_pipeline_ep_pdf_thumbnail_generator.id
  storage_account_name          = azurerm_storage_account.sa_pdf_thumbnail_generator.name
  storage_account_access_key    = azurerm_storage_account.sa_pdf_thumbnail_generator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_pdfthumbnailgenerator_subnet.id
  tags                          = local.common_tags
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  builtin_logging_enabled       = false


  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_pdf_thumbnail_generator.primary_connection_string
    "AzureWebJobs.SlidingClearDown.Disabled"          = var.thumbnail_generator_sliding_clear_down.disabled
    "BlobServiceContainerName"                        = var.blob_service_container_name
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "SlidingClearDownBatchSize"                       = var.thumbnail_generator_sliding_clear_down.batch_size
    "SlidingClearDownSchedule"                        = var.thumbnail_generator_sliding_clear_down.schedule
    "SlidingClearDownInputHours"                      = var.thumbnail_generator_sliding_clear_down.input_hours
    "ThumbnailGeneratorTaskHub"                       = "fapolaris${var.env != "prod" ? var.env : ""}pdfthumbgen"
    "BlobServiceContainerNameThumbnails"              = var.blob_thumbnails_container_name
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet-isolated"
    "HostType"                                        = "Production"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_pdf_thumbnail_generator.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_pdf_thumbnail_generator_file_share.name
    "WEBSITE_DNS_ALT_SERVER"                          = var.dns_alt_server
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "1"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"             = "10"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITE_WARMUP_PATH"                             = "/api/status"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"          = "1"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
  }

  sticky_settings {
    app_setting_names = ["ThumbnailGeneratorTaskHub", "HostType", "AzureWebJobs.SlidingClearDown.Disabled"]
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    vnet_route_all_enabled                 = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    application_stack {
      dotnet_version = "v8.0"
    }
    always_on                         = true
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
    use_32_bit_worker                 = false
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
      app_settings["AzureWebJobsStorage"],
      app_settings["AzureWebJobs.SlidingClearDown.Disabled"],
      app_settings["BlobServiceContainerName"],
      app_settings["BlobServiceUrl"],
      app_settings["SlidingClearDownBatchSize"],
      app_settings["SlidingClearDownSchedule"],
      app_settings["SlidingClearDownInputHours"],
      app_settings["BlobServiceContainerNameThumbnails"],
      app_settings["HostType"],
      app_settings["SCALE_CONTROLLER_LOGGING_ENABLED"],
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
      app_settings["WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"],
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"]
    ]
  }
}

module "azurerm_app_reg_fa_pdf_thumbnail_generator" { # Note, app roles are currently being managed outside of terraform and it's functionality has been commented out from the module.
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "fa-${local.global_resource_name}-pdf-thumb-gen-appreg"
  identifier_uris         = ["api://fa-${local.global_resource_name}-pdf-thumb-gen"]
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

resource "azuread_application_password" "faap_fa_pdf_thumbnail_generator_app_service" {
  application_object_id = module.azurerm_app_reg_fa_pdf_thumbnail_generator.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_fa_pdf_thumbnail_generator" { # Note, app roles are currently being managed outside of terraform and it's functionality has been commented out from the module.
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_fa_pdf_thumbnail_generator.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_pdf_thumbnail_generator_pw" {
  service_principal_id = module.azurerm_service_principal_fa_pdf_thumbnail_generator.object_id
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_pdf_thumbnail_generator_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_pdf_thumbnail_generator.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_thumbnail_generator_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_thumbnail_generator.name}-pe"
  resource_group_name = azurerm_resource_group.rg_coordinator.name
  location            = azurerm_resource_group.rg_coordinator.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_thumbnail_generator.name}-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_thumbnail_generator.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Storage Account Permissions
resource "azurerm_role_assignment" "ra_blob_delegator_sa_pdf_thumbnail_generator" {
  scope                = azurerm_storage_account.sa.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_windows_function_app.fa_pdf_thumbnail_generator.identity[0].principal_id

  depends_on = [
    azurerm_storage_account.sa,
    azurerm_windows_function_app.fa_pdf_thumbnail_generator
  ]
}

resource "azurerm_role_assignment" "ra_blob_delegator_pdf_thumbnail_generator" {
  scope                = azurerm_storage_account.sa_pdf_thumbnail_generator.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_windows_function_app.fa_pdf_thumbnail_generator.identity[0].principal_id

  depends_on = [
    azurerm_storage_account.sa_pdf_thumbnail_generator,
    azurerm_windows_function_app.fa_pdf_thumbnail_generator
  ]
}

resource "azurerm_role_assignment" "ra_document_blob_data_contributor_pdf_thumbnail_generator" {
  scope                = azurerm_storage_container.container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_windows_function_app.fa_pdf_thumbnail_generator.identity[0].principal_id

  depends_on = [
    azurerm_storage_account.sa,
    azurerm_storage_container.container,
    azurerm_windows_function_app.fa_pdf_thumbnail_generator
  ]
}

resource "azurerm_role_assignment" "ra_thumbnail_blob_data_contributor_pdf_thumbnail_generator" {
  scope                = azurerm_storage_container.thumbnails.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_windows_function_app.fa_pdf_thumbnail_generator.identity[0].principal_id

  depends_on = [
    azurerm_storage_account.sa,
    azurerm_storage_container.thumbnails,
    azurerm_windows_function_app.fa_pdf_thumbnail_generator
  ]
}