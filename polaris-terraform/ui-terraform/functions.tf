#################### Functions ####################
data "azuread_client_config" "current" {}

resource "azurerm_linux_function_app" "fa_polaris" {
  name                       = "fa-${local.resource_name}-gateway"
  location                   = azurerm_resource_group.rg_polaris.location
  resource_group_name        = azurerm_resource_group.rg_polaris.name
  service_plan_id            = azurerm_service_plan.asp_polaris.id
  storage_account_name       = azurerm_storage_account.sacpspolaris.name
  storage_account_access_key = azurerm_storage_account.sacpspolaris.primary_access_key
  virtual_network_subnet_id  = data.azurerm_subnet.polaris_gateway_subnet.id
  functions_extension_version                    = "~4"
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                       = "dotnet"
    "APPINSIGHTS_INSTRUMENTATIONKEY"                 = azurerm_application_insights.ai_polaris.instrumentation_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"            = ""
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                = ""
    "WEBSITE_CONTENTOVERVNET"                        = "1"
    "WEBSITE_DNS_SERVER"                             = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"       = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                           = azapi_resource.polaris_sacpspolaris_gateway_file_share.name
    "AzureWebJobsStorage"                            = azurerm_storage_account.sacpspolaris.primary_connection_string
    "OnBehalfOfTokenTenantId"                        = data.azurerm_client_config.current.tenant_id
    "OnBehalfOfTokenClientId"                        = module.azurerm_app_reg_fa_polaris.client_id
    "OnBehalfOfTokenClientSecret"                    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.kvs_fa_polaris_client_secret.id})"
    "PolarisPipelineCoordinatorBaseUrl"              = "https://fa-${local.pipeline_resource_name}-coordinator.azurewebsites.net/api/"
    "PolarisPipelineCoordinatorScope"                = "api://fa-${local.pipeline_resource_name}-coordinator/user_impersonation"
    "PolarisPipelineCoordinatorFunctionAppKey"       = data.azurerm_function_app_host_keys.fa_pipeline_coordinator_host_keys.default_function_key
    "PolarisPipelineRedactPdfScope"                  = "api://fa-${local.pipeline_resource_name}-pdf-generator/user_impersonation"
    "PolarisPipelineRedactPdfBaseUrl"                = "https://fa-${local.pipeline_resource_name}-pdf-generator.azurewebsites.net/api/"
    "PolarisPipelineRedactPdfFunctionAppKey"         = data.azurerm_function_app_host_keys.fa_pipeline_pdf_generator_host_keys.default_function_key
    "BlobServiceUrl"                                 = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobContainerName"                              = "documents"
    "BlobExpirySecs"                                 = 3600
    "BlobUserDelegationKeyExpirySecs"                = 3600
    "searchClient__EndpointUrl"                      = "https://ss-${local.pipeline_resource_name}.search.windows.net"
    "searchClient__AuthorizationKey"                 = data.azurerm_search_service.pipeline_ss.primary_key
    "searchClient__IndexName"                        = "lines-index"
    "CallingAppValidAudience"                        = var.polaris_webapp_details.valid_audience
    "CallingAppValidScopes"                          = var.polaris_webapp_details.valid_scopes
	"CallingAppValidRoles"                           = var.polaris_webapp_details.valid_roles
    "Ddei__BaseUrl"                                  = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "Ddei__AccessKey"                                = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key,
    "Ddei__DefaultScope"                             = "api://fa-${local.ddei_resource_name}/user_impersonation"
  }
	
  site_config {
    always_on        = true
    ip_restriction   = []
    cors {
      allowed_origins     = ["https://as-web-${local.resource_name}.azurewebsites.net", var.env == "dev" ? "http://localhost:3000" : ""]
      support_credentials = true
    }
    vnet_route_all_enabled = true
  }

  tags = {
    environment = var.environment_tag
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
      client_id                  = module.azurerm_app_reg_fa_polaris.client_id
      client_secret              = azuread_application_password.asap_web_polaris_app_service.value 
      allowed_audiences          = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-gateway"]
    }
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
    ]
  }
  
  depends_on = [azurerm_storage_account.sacpspolaris, azapi_resource.polaris_sacpspolaris_gateway_file_share]
}

module "azurerm_app_reg_fa_polaris" {
  source  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name = "fa-${local.resource_name}-gateway-appreg"
  identifier_uris = ["https://CPSGOVUK.onmicrosoft.com/fa-${local.resource_name}-gateway"]
  owners = [data.azuread_client_config.current.object_id]
  prevent_duplicate_names = true
  #use this code for adding scopes
  api = {
    mapped_claims_enabled          = false
    requested_access_token_version = 1
    known_client_applications      = []
    oauth2_permission_scope = [{
      admin_consent_description  = "Allow the calling application to make requests of the ${local.resource_name} Gateway"
      admin_consent_display_name = "Call the ${local.resource_name} Gateway"
      id                         = element(random_uuid.random_id[*].result, 0)
      type                       = "Admin"
      user_consent_description   = "Interact with the ${local.resource_name} Gateway on-behalf of the calling user"
      user_consent_display_name  = "Interact with the ${local.resource_name} Gateway"
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
      # Coordinator
      resource_app_id = data.azuread_application.fa_pipeline_coordinator.id
      resource_access = [{
        # User Impersonation Scope
        id   = data.azuread_application.fa_pipeline_coordinator.oauth2_permission_scope_ids["user_impersonation"]
        type = "Scope"
      }]
    },
    {
      # Pdf Generator
      resource_app_id = data.azuread_application.fa_pipeline_pdf_generator.id
      resource_access = [{
        # User Impersonation Scope
        id   = data.azuread_application.fa_pipeline_pdf_generator.oauth2_permission_scope_ids["user_impersonation"]
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
    redirect_uris = ["https://fa-${local.resource_name}-gateway.azurewebsites.net/.auth/login/aad/callback"]
    implicit_grant = {
      id_token_issuance_enabled = true
    }
  }
  tags = [var.environment_tag, "terraform"]
}

resource "azuread_application_password" "faap_polaris_app_service" {
  application_object_id = module.azurerm_app_reg_fa_polaris.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_sp_polaris_gateway" {
  source         = "./modules/terraform-azurerm-azuread_service_principal"
  application_id = module.azurerm_app_reg_fa_polaris.client_id
  app_role_assignment_required = false
  owners         = [data.azurerm_client_config.current.object_id]
}

resource "azuread_service_principal_password" "sp_polaris_gateway_pw" {
  service_principal_id = module.azurerm_service_principal_sp_polaris_gateway.object_id
}

resource "azuread_application_pre_authorized" "fapre_fa_coordinator" {
  application_object_id = data.azuread_application.fa_pipeline_coordinator.object_id
  authorized_app_id     = module.azurerm_app_reg_fa_polaris.client_id
  permission_ids        = [data.azuread_application.fa_pipeline_coordinator.oauth2_permission_scope_ids["user_impersonation"]]
  depends_on = [module.azurerm_app_reg_fa_polaris]
}

resource "azuread_application_pre_authorized" "fapre_fa_pdf-generator" {
  application_object_id = data.azuread_application.fa_pipeline_pdf_generator.object_id
  authorized_app_id     = module.azurerm_app_reg_fa_polaris.client_id
  permission_ids        = [data.azuread_application.fa_pipeline_pdf_generator.oauth2_permission_scope_ids["user_impersonation"]]
  depends_on = [module.azurerm_app_reg_fa_polaris]
}

resource "azuread_application_pre_authorized" "fapre_fa_ddei" {
  application_object_id = data.azuread_application.fa_ddei.object_id
  authorized_app_id     = module.azurerm_app_reg_fa_polaris.client_id
  permission_ids        = [data.azuread_application.fa_ddei.oauth2_permission_scope_ids["user_impersonation"]]
  depends_on = [module.azurerm_app_reg_fa_polaris]
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_pdf_generator_grant_access" {
  service_principal_object_id          = module.azurerm_service_principal_sp_polaris_gateway.object_id
  resource_service_principal_object_id = data.azuread_service_principal.fa_pdf_generator_service_principal.object_id
  claim_values                         = ["user_impersonation"]
  depends_on = [module.azurerm_app_reg_fa_polaris]
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_ddei_grant_access" {
  service_principal_object_id          = module.azurerm_service_principal_sp_polaris_gateway.object_id
  resource_service_principal_object_id = data.azuread_service_principal.fa_ddei_service_principal.object_id
  claim_values                         = ["user_impersonation"]
  depends_on = [module.azurerm_app_reg_fa_polaris]
}