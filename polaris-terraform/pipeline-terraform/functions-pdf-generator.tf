#################### Functions ####################

resource "azurerm_windows_function_app" "fa_pdf_generator" {
  name                       = "fa-${local.resource_name}-pdf-generator"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  service_plan_id            = azurerm_service_plan.aspw.id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  functions_extension_version                 = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                = "dotnet"
    "APPINSIGHTS_INSTRUMENTATIONKEY"          = azurerm_application_insights.ai.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"     = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"         = ""
    "AzureWebJobsStorage"                     = azurerm_storage_account.sa.primary_connection_string
    "BlobServiceUrl"                          = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                = "documents"
    "CallingAppTenantId"                      = data.azurerm_client_config.current.tenant_id
    "CallingAppValidAudience"                 = "api://fa-${local.resource_name}-pdf-generator"
    "SearchClientAuthorizationKey"            = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                 = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                   = jsondecode(file("search-index-definition.json")).name
    "DocumentsRepositoryBaseUrl"              = "https://fa-${local.ddei_resource_name}.azurewebsites.net/api/"
    "GetDocumentUrl"                          = "urns/{0}/cases/{1}/documents/{2}/{3}?code=${data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key}"
    "OnBehalfOfTokenTenantId"                 = data.azurerm_client_config.current.tenant_id
    "OnBehalfOfTokenClientId"                 = module.azurerm_app_reg_fa_pdf_generator.client_id
    "OnBehalfOfTokenClientSecret"             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_pdf_generator_client_secret.id})"
    "DdeiScope"                               = "api://fa-${local.ddei_resource_name}/user_impersonation"
  }
  https_only                 = true

  site_config {
    always_on      = true
    ip_restriction = []
    ftps_state     = "FtpsOnly"
    http2_enabled = true
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
      client_id         = module.azurerm_app_reg_fa_pdf_generator.client_id
      client_secret     = azuread_application_password.faap_fa_pdf_generator_app_service.value
      allowed_audiences = ["api://fa-${local.resource_name}-pdf-generator"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }
}

module "azurerm_app_reg_fa_pdf_generator" {
  source  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name = "fa-${local.resource_name}-pdf-generator-appreg"
  identifier_uris = ["api://fa-${local.resource_name}-pdf-generator"]
  prevent_duplicate_names = true
  #use this code for adding scopes
  api = {
    mapped_claims_enabled          = false
    requested_access_token_version = 1
    known_client_applications      = []
    oauth2_permission_scope = [{
      admin_consent_description  = "Allow the calling application to make requests of the ${local.resource_name} PDF Generator"
      admin_consent_display_name = "Call the ${local.resource_name} PDF Generator"
      id                         = element(random_uuid.random_id[*].result, 1)
      type                       = "Admin"
      user_consent_description   = "Interact with the ${local.resource_name} Polaris PDF Generator on-behalf of the calling user"
      user_consent_display_name  = "Interact with the ${local.resource_name} Polaris PDF Generator"
      value                      = "user_impersonation"
    }]
  }
  #use this code for adding app_roles
  app_role = [
    {
      allowed_member_types  = ["Application"]
      description          = "Can create PDF resources using the ${local.resource_name} PDF Generator"
      display_name         = "Create PDF resources"
      id                   = element(random_uuid.random_id[*].result, 2)
      value                = "application.create"
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
  },
    {
      # DDEI
      resource_app_id = data.azuread_application.fa_ddei.id
      resource_access = [{
        # User Impersonation Scope
        id   = data.azuread_application.fa_ddei.oauth2_permission_scope_ids["user_impersonation"]
        type = "Scope"
      }]
    }]
  web = {
    redirect_uris = ["https://fa-${local.resource_name}-pdf-generator.azurewebsites.net/.auth/login/aad/callback"]
    implicit_grant = {
      id_token_issuance_enabled     = true
    }
  }
  tags = ["fa-${local.resource_name}-pdf-generator", "terraform"]
}

data "azurerm_function_app_host_keys" "ak_pdf_generator" {
  name                = "fa-${local.resource_name}-pdf-generator"
  resource_group_name = azurerm_resource_group.rg.name
  depends_on = [azurerm_windows_function_app.fa_pdf_generator]
}

module "azurerm_app_pre_authorized" {
  source                = "./modules/terraform-azurerm-azure_ad_application_preauthorized"
  
  # application object id of authorized application
  application_object_id = module.azurerm_app_reg_fa_pdf_generator.object_id

  # application id of Client application
  authorized_app_id     = module.azurerm_app_reg_fa_coordinator.client_id

  # permissions to assign
  permission_ids        = [module.azurerm_app_reg_fa_pdf_generator.oauth2_permission_scope_ids["user_impersonation"]]
}

module "azurerm_app_pre_authorized_ddei" {
  source                = "./modules/terraform-azurerm-azure_ad_application_preauthorized"

  # application object id of authorized application
  application_object_id = module.azurerm_app_reg_fa_pdf_generator.object_id

  # application id of Client application
  authorized_app_id     = data.azuread_application.fa_ddei.application_id

  # permissions to assign
  permission_ids        = [module.azurerm_app_reg_fa_pdf_generator.oauth2_permission_scope_ids["user_impersonation"]]
}

resource "azuread_application_password" "faap_fa_pdf_generator_app_service" {
  application_object_id = module.azurerm_app_reg_fa_pdf_generator.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_fa_pdf_generator" {
  source         = "./modules/terraform-azurerm-azuread_service_principal"
  application_id = module.azurerm_app_reg_fa_pdf_generator.client_id
  app_role_assignment_required = false
  owners         = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_fa_pdf_generator_pw" {
  service_principal_id = module.azurerm_service_principal_fa_pdf_generator.object_id
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_pdf_generator_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_fa_pdf_generator.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_pdf_generator_grant_access_to_ddei" {
  service_principal_object_id          = module.azurerm_service_principal_fa_pdf_generator.object_id
  resource_service_principal_object_id = data.azuread_application.fa_ddei.id
  claim_values                         = ["user_impersonation"]
}