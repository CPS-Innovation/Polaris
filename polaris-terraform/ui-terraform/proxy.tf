resource "azurerm_linux_web_app" "polaris_proxy" {
  name                = "${local.resource_name}-cmsproxy"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_service_plan.asp_polaris.location
  service_plan_id     = azurerm_service_plan.asp_polaris.id
  site_config {
    application_stack {
      docker_image     = "registry.hub.docker.com/library/nginx"
      docker_image_tag = "1.23.3"
    }
  }
  auth_settings {
    enabled = true
    unauthenticated_client_action = "AllowAnonymous"
  }
  app_settings = {
    UPSTREAM_HOST                         = "github.com"
    RESOLVER                              = "10.2.64.10"
    NGINX_ENVSUBST_OUTPUT_DIR             = "/etc/nginx"
    API_ENDPOINT                          = "api.github.com"
    FORCE_REFRESH_CONFIG                  = "${md5(file("nginx.conf"))}:${md5(file("nginx.js"))}"
  }
  storage_account {
    access_key   = azurerm_storage_account.sacpspolaris.primary_access_key
    account_name = azurerm_storage_account.sacpspolaris.name
    name         = "config"
    share_name   = azurerm_storage_container.polaris_proxy_content.name
    type         = "AzureBlob"
    mount_path   = "/etc/nginx/templates"
  }
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