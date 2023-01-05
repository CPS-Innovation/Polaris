#################### Functions ####################

resource "azurerm_function_app" "fa_coordinator" {
  name                       = "fa-${local.resource_name}-coordinator"
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
    "CoordinatorOrchestratorTimeoutSecs"      = "600"
    "OnBehalfOfTokenTenantId"                 = data.azurerm_client_config.current.tenant_id
    "OnBehalfOfTokenClientId"                 = module.azurerm_app_reg_fa_coordinator.client_id
    "OnBehalfOfTokenClientSecret"             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_coordinator_client_secret.id})"
    "PdfGeneratorScope"                       = "api://fa-${local.resource_name}-pdf-generator/.default"
    "PdfGeneratorUrl"                         = "https://fa-${local.resource_name}-pdf-generator.azurewebsites.net/api/generate?code=${data.azurerm_function_app_host_keys.ak_pdf_generator.default_function_key}"
    "TextExtractorScope"                      = "api://fa-${local.resource_name}-text-extractor/.default"
    "TextExtractorUrl"                        = "https://fa-${local.resource_name}-text-extractor.azurewebsites.net/api/extract?code=${data.azurerm_function_app_host_keys.ak_text_extractor.default_function_key}"
    "CallingAppTenantId"                      = data.azurerm_client_config.current.tenant_id
    "CallingAppValidAudience"                 = "api://fa-${local.resource_name}-coordinator"
    "DocumentsRepositoryBaseUrl"              = var.ddei_config.base_url
    "ListDocumentsUrl"                        = "urns/{0}/cases/{1}/documents?code=${var.ddei_config.list_documents_function_key}"
    "DocumentEvaluatorScope"                  = "api://fa-${local.resource_name}-document-evaluator/.default"
    "DocumentEvaluatorUrl"                    = "https://fa-${local.resource_name}-document-evaluator.azurewebsites.net/api/processEvaluatedDocuments?code=${data.azurerm_function_app_host_keys.ak_document_evaluator.default_function_key}"
  }
  https_only                 = true

  site_config {
    always_on      = true
    ip_restriction = []
    ftps_state     = "FtpsOnly"
    http2_enabled = true

    cors {
      allowed_origins = []
    }
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
      client_id         = module.azurerm_app_reg_fa_coordinator.client_id
      client_secret     = azuread_application_password.faap_fa_coordinator_app_service.value
      allowed_audiences = ["api://fa-${local.resource_name}-coordinator"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }

  # depends_on = [
  #   data.azurerm_function_app_host_keys.ak_pdf_generator,
  #   data.azurerm_function_app_host_keys.ak_text_extractor,
  #   data.azurerm_function_app_host_keys.ak_indexer
  # ]
}

data "azurerm_function_app_host_keys" "ak_coordinator" {
  name                = "fa-${local.resource_name}-coordinator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_function_app.fa_coordinator]
}

module "azurerm_app_reg_fa_coordinator" {
  source  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name = "fa-${local.resource_name}-coordinator-appreg"
  identifier_uris = ["api://fa-${local.resource_name}-coordinator"]
  prevent_duplicate_names = true
  #use this code for adding scopes
  api = {
    mapped_claims_enabled          = false
    requested_access_token_version = 1
    known_client_applications      = []
    oauth2_permission_scope = [{
      admin_consent_description  = "Allow the calling application to instigate the ${local.resource_name} ${local.resource_name} coordinator"
      admin_consent_display_name = "Start the ${local.resource_name} Pipeline coordinator"
      id                         = element(random_uuid.random_id[*].result, 0)
      type                       = "Admin"
      user_consent_description   = "Interact with the ${local.resource_name} Polaris Pipeline on-behalf of the calling user"
      user_consent_display_name  = "Interact with the ${local.resource_name} Polaris Pipeline"
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
    },
    {
      # Pdf Generator
      resource_app_id = module.azurerm_app_reg_fa_pdf_generator.client_id
      resource_access = [{
          # User Impersonation Scope
          id   = module.azurerm_app_reg_fa_pdf_generator.oauth2_permission_scope_ids["user_impersonation"]
          type = "Scope"
        },
        {
          # Application.Create Role
          id   = module.azurerm_app_reg_fa_pdf_generator.app_role_ids["application.create"]
          type = "Role"
        }]
    }]
  web = {
    redirect_uris = ["https://fa-${local.resource_name}-coordinator.azurewebsites.net/.auth/login/aad/callback",
      "https://getpostman.com/oauth2/callback"]
    implicit_grant = {
      id_token_issuance_enabled     = true
    }
  }
  tags = ["fa-${local.resource_name}-coordinator", "terraform"]
}

module "azurerm_service_principal_fa_coordinator" {
  source         = "./modules/terraform-azurerm-azuread_service_principal"
  application_id = module.azurerm_app_reg_fa_coordinator.client_id
  app_role_assignment_required = false
  owners         = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_coordinator_pw" {
  service_principal_id = module.azurerm_service_principal_fa_coordinator.object_id
}

resource "azuread_application_password" "faap_fa_coordinator_app_service" {
  application_object_id = module.azurerm_app_reg_fa_coordinator.object_id
  end_date_relative     = "17520h"
}

resource "azuread_service_principal_delegated_permission_grant" "rumpole_coordinator_grant_access" {
  service_principal_object_id          = module.azurerm_service_principal_fa_coordinator.object_id
  resource_service_principal_object_id = module.azurerm_service_principal_fa_pdf_generator.object_id
  claim_values                         = ["user_impersonation"]
}

resource "azuread_service_principal_delegated_permission_grant" "rumpole_document_evaluator_grant_access" {
  service_principal_object_id          = module.azurerm_service_principal_fa_coordinator.object_id
  resource_service_principal_object_id = module.azurerm_service_principal_fa_document_evaluator.object_id
  claim_values                         = ["user_impersonation"]
}

resource "azuread_service_principal_delegated_permission_grant" "rumpole_coordinator_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_coordinator.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}