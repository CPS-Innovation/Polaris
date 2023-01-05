#################### Functions ####################

resource "azurerm_function_app" "fa_document_evaluator" {
  name                       = "fa-${local.resource_name}-document-evaluator"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.asp.id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  os_type                    = "linux"
  version                    = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "BlobServiceUrl"                          = azurerm_storage_account.sa.primary_blob_endpoint
    "BlobServiceContainerName"                = azurerm_storage_container.container.name
    "SearchClientAuthorizationKey"            = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                 = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                   = jsondecode(file("search-index-definition.json")).name
    "DocumentEvaluatorQueueUrl"               = "https://sacps${var.env != "prod" ? var.env : ""}rumpolepipeline.queue.core.windows.net/{0}"
    "UpdateSearchIndexByVersionQueueName"     = var.queue_config.update_search_index_by_version_queue_name
  }
  https_only                 = true

  site_config {
    always_on      = true
    ip_restriction = []
    ftps_state     = "FtpsOnly"
    http2_enabled = true
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "RedirectToLoginPage"
    default_provider              = "AzureActiveDirectory"
    active_directory {
      client_id         = module.azurerm_app_reg_fa_document_evaluator.client_id
      client_secret     = azuread_application_password.faap_fa_document_evaluator_app_service.value
      allowed_audiences = ["api://fa-${local.resource_name}-document-evaluator"]
    }
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }
}

module "azurerm_app_reg_fa_document_evaluator" {
  source  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name = "fa-${local.resource_name}-document-evaluator-appreg"
  identifier_uris = ["api://fa-${local.resource_name}-document-evaluator"]
  prevent_duplicate_names = true
  #use this code for adding scopes
  api = {
    mapped_claims_enabled          = false
    requested_access_token_version = 1
    known_client_applications      = []
    oauth2_permission_scope = [{
      admin_consent_description  = "Allow the calling application to make requests of the ${local.resource_name} Document Evaluator"
      admin_consent_display_name = "Call the ${local.resource_name} Document Evaluator"
      id                         = element(random_uuid.random_id[*].result, 1)
      type                       = "Admin"
      user_consent_description   = "Interact with the ${local.resource_name} Polaris Document Evaluator on-behalf of the calling user"
      user_consent_display_name  = "Interact with the ${local.resource_name} Polaris Document Evaluator"
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
    redirect_uris = ["https://fa-${local.resource_name}-document-evaluator.azurewebsites.net/.auth/login/aad/callback"]
    implicit_grant = {
      id_token_issuance_enabled     = true
    }
  }
  tags = ["fa-${local.resource_name}-document-evaluator", "terraform"]
}

data "azurerm_function_app_host_keys" "ak_document_evaluator" {
  name                = "fa-${local.resource_name}-document-evaluator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_function_app.fa_document_evaluator]
}

module "azurerm_app_pre_authorized_fa_document_evaluator" {
  source                = "./modules/terraform-azurerm-azure_ad_application_preauthorized"

  # application object id of authorized application
  application_object_id = module.azurerm_app_reg_fa_document_evaluator.object_id

  # application id of Client application
  authorized_app_id     = module.azurerm_app_reg_fa_coordinator.client_id

  # permissions to assign
  permission_ids        = [module.azurerm_app_reg_fa_document_evaluator.oauth2_permission_scope_ids["user_impersonation"]]
}

resource "azuread_application_password" "faap_fa_document_evaluator_app_service" {
  application_object_id = module.azurerm_app_reg_fa_document_evaluator.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_fa_document_evaluator" {
  source         = "./modules/terraform-azurerm-azuread_service_principal"
  application_id = module.azurerm_app_reg_fa_document_evaluator.client_id
  app_role_assignment_required = false
  owners         = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_document_evaluator_pw" {
  service_principal_id = module.azurerm_service_principal_fa_document_evaluator.object_id
}

resource "azuread_service_principal_delegated_permission_grant" "rumpole_document_evaluator_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_document_evaluator.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}