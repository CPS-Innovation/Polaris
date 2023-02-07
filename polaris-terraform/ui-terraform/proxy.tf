resource "azurerm_linux_web_app" "polaris_proxy" {
  name                = "${local.resource_name}-cmsproxy"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_service_plan.asp_polaris.location
  service_plan_id     = azurerm_service_plan.asp_polaris.id
  app_settings = {
    "WEBSITE_CONTENTOVERVNET"                  = "1"
    "WEBSITE_DNS_SERVER"                       = "10.2.64.10"
    "WEBSITE_DNS_ALT_SERVER"                   = "10.3.64.10"
    "APPINSIGHTS_INSTRUMENTATIONKEY"           = azurerm_application_insights.ai_polaris.instrumentation_key
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                     = azapi_resource.polaris_sacpspolaris_proxy_file_share.name
    DOCKER_REGISTRY_SERVER_URL                 = "https://${data.azurerm_container_registry.polaris_container_registry.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME            = data.azurerm_container_registry.polaris_container_registry.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD            = data.azurerm_container_registry.polaris_container_registry.admin_password
    UPSTREAM_HOST                              = "10.2.177.14"
    RESOLVER                                   = "10.2.64.10 10.3.64.10"
    NGINX_ENVSUBST_OUTPUT_DIR                  = "/etc/nginx"
    API_ENDPOINT                               = "${azurerm_linux_function_app.fa_polaris.name}.azurewebsites.net/api"
    FORCE_REFRESH_CONFIG                       = "${md5(file("nginx.conf"))}:${md5(file("nginx.js"))}"
  }
  site_config {
    application_stack {
      docker_image     = "registry.hub.docker.com/library/nginx"
      docker_image_tag = "1.23.3"
    }
    always_on              = true
    vnet_route_all_enabled = true
  }
  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "RedirectToLoginPage"
    default_provider              = "AzureActiveDirectory"
    active_directory {
      client_id         = module.azurerm_app_reg_polaris_proxy.client_id
      client_secret     = azuread_application_password.asap_polaris_cms_proxy.value
      allowed_audiences = ["https://CPSGOVUK.onmicrosoft.com/${local.resource_name}-cmsproxy"]
    }
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
      id   = "311a71cc-e848-46a1-bdf8-97ff7156d8e6" # read user
      type = "Scope"
    }]
  }]
  web = {
    homepage_url  = "https://${local.resource_name}-cmsproxy.azurewebsites.net"
    redirect_uris = ["https://getpostman.com/oauth2/callback"]
    implicit_grant = {
      access_token_issuance_enabled = true
    }
  }
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

resource "azurerm_storage_blob" "nginx_config" {
  name                   = "nginx.conf.template"
  content_md5            = md5(file("nginx.conf"))
  storage_account_name   = azurerm_storage_account.sacpspolaris.name
  storage_container_name = azurerm_storage_container.polaris_proxy_content.name
  type                   = "Block"
  source                 = "nginx.conf"
}

resource "azurerm_storage_blob" "nginx_js" {
  name                   = "nginx.js"
  content_md5            = md5(file("nginx.js"))
  storage_account_name   = azurerm_storage_account.sacpspolaris.name
  storage_container_name = azurerm_storage_container.polaris_proxy_content.name
  type                   = "Block"
  source                 = "nginx.js"
}

resource "azurerm_app_service_virtual_network_swift_connection" "polaris_proxy_to_vnet" {
  app_service_id = azurerm_linux_web_app.polaris_proxy.id
  subnet_id      = data.azurerm_subnet.polaris_proxy_subnet.id
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
}

# Create Private Endpoint for SCM site
resource "azurerm_private_endpoint" "polaris_proxy_scm_pe" {
  name                = "${azurerm_linux_web_app.polaris_proxy.name}-scm-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_web_app.polaris_proxy.name}-scm-psc"
    private_connection_resource_id = azurerm_linux_web_app.polaris_proxy.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_proxy_scm_dns_a" {
  name                = "${azurerm_linux_web_app.polaris_proxy.name}.scm"
  zone_name           = data.azurerm_private_dns_zone.dns_zone_apps.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_proxy_scm_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}