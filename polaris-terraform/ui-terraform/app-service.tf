#################### App Service ####################

resource "azurerm_linux_web_app" "as_web_polaris" {
  #checkov:skip=CKV_AZURE_88:Ensure that app services use Azure Files
  #checkov:skip=CKV_AZURE_16:Ensure that Register with Azure Active Directory is enabled on App Service
  #checkov:skip=CKV_AZURE_63:Ensure that App service enables HTTP logging
  #checkov:skip=CKV_AZURE_213:Ensure that App Service configures health check
  #checkov:skip=CKV_AZURE_71:Ensure that Managed identity provider is enabled for app services
  #checkov:skip=CKV_AZURE_17:Ensure the web app has 'Client Certificates (Incoming client certificates)' set
  name                          = "as-web-${local.resource_name}"
  location                      = azurerm_resource_group.rg_polaris.location
  resource_group_name           = azurerm_resource_group.rg_polaris.name
  service_plan_id               = azurerm_service_plan.asp_polaris_spa.id
  https_only                    = true
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_ui_subnet.id
  public_network_access_enabled = false
  
  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                  = data.azurerm_application_insights.global_ai.instrumentation_key
    "HostType"                                        = "Production"
    "REACT_APP_AI_KEY"                                = data.azurerm_application_insights.global_ai.instrumentation_key
    "REACT_APP_CLIENT_ID"                             = module.azurerm_app_reg_as_web_polaris.client_id
    "REACT_APP_FEATURE_FLAG_HTE_EMAILS_ON"            = var.feature_flag_hte_emails_on
    "REACT_APP_FEATURE_FLAG_REDACTION_LOG"            = var.feature_flag_redaction_log
    "REACT_APP_FEATURE_FLAG_REDACTION_LOG_UNDER_OVER" = var.feature_flag_redaction_log_under_over
    "REACT_APP_FEATURE_FLAG_FULL_SCREEN"              = var.feature_flag_full_screen
    "REACT_APP_GATEWAY_BASE_URL"                      = ""
    "REACT_APP_GATEWAY_SCOPE"                         = "https://CPSGOVUK.onmicrosoft.com/${azurerm_linux_function_app.fa_polaris.name}/user_impersonation"
    "REACT_APP_IS_REDACTION_SERVICE_OFFLINE"          = var.is_redaction_service_offline
    "REACT_APP_PRIVATE_BETA_SIGN_UP_URL"              = var.private_beta.sign_up_url
    "REACT_APP_PRIVATE_BETA_USER_GROUP"               = var.private_beta.user_group
    "REACT_APP_PRIVATE_BETA_REDACTION_LOG_USER_GROUP" = var.private_beta.redaction_log_user_group
    "REACT_APP_REDACTION_LOG_USER_GROUP"              = var.redaction_log_user_group
    "REACT_APP_REAUTH_REDIRECT_URL"                   = "/polaris?polaris-ui-url="
    "REACT_APP_REDACTION_LOG_BASE_URL"                = "https://fa-${local.redaction_log_resource_name}-reporting.azurewebsites.net"
    "REACT_APP_REDACTION_LOG_SCOPE"                   = "https://CPSGOVUK.onmicrosoft.com/fa-${local.redaction_log_resource_name}-reporting/user_impersonation"
    "REACT_APP_SURVEY_LINK"                           = "https://www.smartsurvey.co.uk/s/DG5B6G/"
    "REACT_APP_TENANT_ID"                             = data.azurerm_client_config.current.tenant_id
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.polaris_sacpspolaris_ui_file_share.name
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"             = "10"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/polaris-ui/build-version.txt"
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITE_WARMUP_PATH"                             = "/polaris-ui/build-version.txt"
  }

  sticky_settings {
    app_setting_names = ["HostType"]
  }

  site_config {
    ftps_state    = "FtpsOnly"
    http2_enabled = true
    # The -s in npx serve -s is very important.  It allows any url that hits the app
    #  to be served from the root index.html.  This is important as it accomodates any
    #  sub directory that the app may be hosted with, or none at all.
    app_command_line       = "node polaris-ui/subsititute-config.js; npx serve -s"
    always_on              = true
    vnet_route_all_enabled = true

    application_stack {
      node_version = "18-lts"
    }
  }

  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    default_provider       = "AzureActiveDirectory"
    unauthenticated_action = "AllowAnonymous"
    excluded_paths         = ["/status"]

    # our default_provider:
    active_directory_v2 {
      tenant_auth_endpoint = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      #checkov:skip=CKV_SECRET_6:Base64 High Entropy String - Misunderstanding of setting "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = module.azurerm_app_reg_as_web_polaris.client_id
    }

    # use a store for tokens (az blob storage backed)
    login {
      token_store_enabled = true
    }
  }

  logs {
    detailed_error_messages = true
    failed_request_tracing  = true
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }
}

module "azurerm_app_reg_as_web_polaris" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "as-web-${local.resource_name}-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/as-web-${local.resource_name}"]
  owners                  = [data.azuread_service_principal.terraform_service_principal.object_id]
  prevent_duplicate_names = true
  group_membership_claims = ["ApplicationGroup"]
  optional_claims = {
    access_token = {
      name = "groups"
    }
    id_token = {
      name = "groups"
    }
    saml2_token = {
      name = "groups"
    }
  }
  #use this code for adding api permissions
  required_resource_access = [{
    # Microsoft Graph
    resource_app_id = "00000003-0000-0000-c000-000000000000"
    resource_access = [{
      id   = "311a71cc-e848-46a1-bdf8-97ff7156d8e6" # read user
      type = "Scope"
    }]
    },
    {
      resource_app_id = module.azurerm_app_reg_fa_polaris.client_id
      resource_access = [{
        id   = module.azurerm_app_reg_fa_polaris.oauth2_permission_scope_ids["user_impersonation"]
        type = "Scope"
      }]
    },
    {
      resource_app_id = data.azuread_application.fa_redaction_log_reporting.application_id
      resource_access = [{
        id   = data.azuread_application.fa_redaction_log_reporting.oauth2_permission_scope_ids["user_impersonation"]
        type = "Scope"
      }]
  }]
  single_page_application = {
    redirect_uris = var.env != "prod" ? ["https://as-web-${local.resource_name}.azurewebsites.net/${var.polaris_ui_sub_folder}", "http://localhost:3000/${var.polaris_ui_sub_folder}", "https://${local.resource_name}-cmsproxy.azurewebsites.net/${var.polaris_ui_sub_folder}", "https://${local.resource_name}-notprod.cps.gov.uk/${var.polaris_ui_sub_folder}"] : ["https://as-web-${local.resource_name}.azurewebsites.net/${var.polaris_ui_sub_folder}", "https://${local.resource_name}-cmsproxy.azurewebsites.net/${var.polaris_ui_sub_folder}", "https://${local.resource_name}.cps.gov.uk/${var.polaris_ui_sub_folder}"]
  }
  api = {
    mapped_claims_enabled          = true
    requested_access_token_version = 1
  }
  web = {
    homepage_url  = "https://as-web-${local.resource_name}.azurewebsites.net"
    redirect_uris = ["https://getpostman.com/oauth2/callback"]
    implicit_grant = {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }
  tags = ["terraform"]
}

resource "azuread_application_password" "asap_web_polaris_app_service" {
  application_object_id = module.azurerm_app_reg_as_web_polaris.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_sp_polaris_web" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_as_web_polaris.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
  depends_on                   = [module.azurerm_app_reg_as_web_polaris]
}

resource "azuread_service_principal_password" "sp_polaris_web_pw" {
  service_principal_id = module.azurerm_service_principal_sp_polaris_web.object_id
  depends_on           = [module.azurerm_service_principal_sp_polaris_web]
}

resource "azuread_application_pre_authorized" "fapre_polaris_web" {
  application_object_id = module.azurerm_app_reg_fa_polaris.object_id
  authorized_app_id     = module.azurerm_app_reg_as_web_polaris.client_id
  permission_ids        = [module.azurerm_app_reg_fa_polaris.oauth2_permission_scope_ids["user_impersonation"]]
  depends_on            = [module.azurerm_app_reg_fa_polaris, module.azurerm_app_reg_as_web_polaris]
}

resource "azuread_application_pre_authorized" "fapre_redaction_log_reporting" {
  application_object_id = data.azuread_application.fa_redaction_log_reporting.object_id
  authorized_app_id     = module.azurerm_app_reg_as_web_polaris.client_id
  permission_ids        = [data.azuread_application.fa_redaction_log_reporting.oauth2_permission_scope_ids["user_impersonation"]]
  depends_on            = [module.azurerm_app_reg_fa_polaris, module.azurerm_app_reg_as_web_polaris]
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_web_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_sp_polaris_web.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
  depends_on                           = [module.azurerm_service_principal_sp_polaris_web, azuread_service_principal.msgraph]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_ui_pe" {
  name                = "${azurerm_linux_web_app.as_web_polaris.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_web_app.as_web_polaris.name}-psc"
    private_connection_resource_id = azurerm_linux_web_app.as_web_polaris.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_ui_dns_a" {
  name                = azurerm_linux_web_app.as_web_polaris.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_ui_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_ui_pe]
}

# Create DNS A Record for SCM site
resource "azurerm_private_dns_a_record" "polaris_ui_scm_dns_a" {
  name                = "${azurerm_linux_web_app.as_web_polaris.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_ui_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_ui_pe]
}
