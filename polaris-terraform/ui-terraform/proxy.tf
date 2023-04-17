resource "azurerm_linux_web_app" "polaris_proxy" {
  name                      = "${local.resource_name}-cmsproxy"
  resource_group_name       = azurerm_resource_group.rg_polaris.name
  location                  = azurerm_resource_group.rg_polaris.location
  service_plan_id           = azurerm_service_plan.asp_polaris_proxy.id
  virtual_network_subnet_id = data.azurerm_subnet.polaris_proxy_subnet.id

  app_settings = {
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                   = "168.63.129.16"
    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.ai_polaris.instrumentation_key
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.polaris_sacpspolaris_proxy_file_share.name
    "UPSTREAM_CMS_DOMAIN_NAME"                 = "10.2.177.14"
    "UPSTREAM_CMS_MODERN_DOMAIN_NAME"          = "10.2.177.55"
    "APP_ENDPOINT_DOMAIN_NAME"                 = "${azurerm_linux_web_app.as_web_polaris.name}.azurewebsites.net"
    "APP_SUBFOLDER_PATH"                       = var.polaris_ui_sub_folder
    "API_ENDPOINT_DOMAIN_NAME"                 = "${azurerm_linux_function_app.fa_polaris.name}.azurewebsites.net"
    "AUTH_HANDOVER_ENDPOINT_DOMAIN_NAME"       = "${azurerm_linux_function_app.fa_polaris_auth_handover.name}.azurewebsites.net"
    "DDEI_ENDPOINT_DOMAIN_NAME"                = "fa-${local.ddei_resource_name}.azurewebsites.net"
    "DDEI_ENDPOINT_FUNCTION_APP_KEY"           = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key
    "ENDPOINT_HTTP_PROTOCOL"                   = "https"
    "RESOLVER"                                 = "${var.dns_server}"
    "DOCKER_REGISTRY_SERVER_URL"               = "https://${data.azurerm_container_registry.polaris_container_registry.login_server}"
    "DOCKER_REGISTRY_SERVER_USERNAME"          = data.azurerm_container_registry.polaris_container_registry.admin_username
    "DOCKER_REGISTRY_SERVER_PASSWORD"          = data.azurerm_container_registry.polaris_container_registry.admin_password
    "NGINX_ENVSUBST_OUTPUT_DIR"                = "/etc/nginx"
    "FORCE_REFRESH_CONFIG"                     = "${md5(file("nginx.conf"))}:${md5(file("nginx.js"))}"
  }
  site_config {
    ftps_state     = "FtpsOnly"
    http2_enabled  = true
    ip_restriction = []
    application_stack {
      docker_image     = "nginx"
      docker_image_tag = "latest"
    }
    always_on                               = true
    vnet_route_all_enabled                  = true
    container_registry_use_managed_identity = true
  }
  auth_settings {
    enabled                       = false
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "AllowAnonymous"
    /*default_provider              = "AzureActiveDirectory"
    active_directory {
      client_id         = module.azurerm_app_reg_polaris_proxy.client_id
      client_secret     = azuread_application_password.asap_polaris_cms_proxy.value
      allowed_audiences = ["https://CPSGOVUK.onmicrosoft.com/${local.resource_name}-cmsproxy"]
    }*/
  }
  storage_account {
    access_key   = azurerm_storage_account.sacpspolaris.primary_access_key
    account_name = azurerm_storage_account.sacpspolaris.name
    name         = "config"
    share_name   = azurerm_storage_container.polaris_proxy_content.name
    type         = "AzureBlob"
    mount_path   = "/etc/nginx/templates"
  }
  identity {
    type = "SystemAssigned"
  }
}

module "azurerm_app_reg_polaris_proxy" {
  source                  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name            = "${local.resource_name}-cmsproxy-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/${local.resource_name}-cmsproxy"]
  owners                  = [data.azuread_service_principal.terraform_service_principal.object_id]
  prevent_duplicate_names = true
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
  /*web = {
    homepage_url  = "https://${local.resource_name}-cmsproxy.azurewebsites.net"
    redirect_uris = ["https://getpostman.com/oauth2/callback", "https://${local.resource_name}-cmsproxy.azurewebsites.net/.auth/login/aad/callback"]
    implicit_grant = {
      id_token_issuance_enabled = true
    }
  }*/
  tags = ["terraform"]
}

resource "azuread_application_password" "asap_polaris_cms_proxy" {
  application_object_id = module.azurerm_app_reg_polaris_proxy.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_sp_polaris_cms_proxy" {
  source                       = "./modules/terraform-azurerm-azuread_service_principal"
  application_id               = module.azurerm_app_reg_polaris_proxy.client_id
  app_role_assignment_required = false
  owners                       = [data.azurerm_client_config.current.object_id]
  depends_on                   = [module.azurerm_app_reg_polaris_proxy]
}

resource "azuread_service_principal_password" "sp_polaris_cms_proxy_pw" {
  service_principal_id = module.azurerm_service_principal_sp_polaris_cms_proxy.object_id
  depends_on           = [module.azurerm_service_principal_sp_polaris_cms_proxy]
}

resource "azurerm_role_assignment" "ra_blob_data_contributor_polaris_proxy" {
  scope                = azurerm_storage_container.polaris_proxy_content.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_web_app.polaris_proxy.identity[0].principal_id
  depends_on           = [azurerm_storage_account.sacpspolaris, azurerm_storage_container.polaris_proxy_content]
}

resource "azurerm_storage_blob" "nginx_config" {
  name                   = "nginx.conf.template"
  content_md5            = md5(file("nginx.conf"))
  storage_account_name   = azurerm_storage_account.sacpspolaris.name
  storage_container_name = azurerm_storage_container.polaris_proxy_content.name
  type                   = "Block"
  source                 = "nginx.conf"
  depends_on             = [azurerm_role_assignment.ra_blob_data_contributor_polaris_proxy]
}

resource "azurerm_storage_blob" "nginx_js" {
  name                   = "nginx.js"
  content_md5            = md5(file("nginx.js"))
  storage_account_name   = azurerm_storage_account.sacpspolaris.name
  storage_container_name = azurerm_storage_container.polaris_proxy_content.name
  type                   = "Block"
  source                 = "nginx.js"
  depends_on             = [azurerm_role_assignment.ra_blob_data_contributor_polaris_proxy]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_proxy_pe" {
  name                = "${azurerm_linux_web_app.polaris_proxy.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_web_app.polaris_proxy.name}-psc"
    private_connection_resource_id = azurerm_linux_web_app.polaris_proxy.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_proxy_dns_a" {
  name                = azurerm_linux_web_app.polaris_proxy.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_proxy_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_proxy_pe]
}

# Create DNS A Record for SCM site
resource "azurerm_private_dns_a_record" "polaris_proxy_scm_dns_a" {
  name                = "${azurerm_linux_web_app.polaris_proxy.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_proxy_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
  depends_on          = [azurerm_private_endpoint.polaris_proxy_pe]
}