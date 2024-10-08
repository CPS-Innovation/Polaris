resource "azurerm_linux_web_app_slot" "polaris_proxy_staging1" {
  #checkov:skip=CKV_AZURE_88:Ensure that app services use Azure Files
  #checkov:skip=CKV_AZURE_13:Ensure App Service Authentication is set on Azure App Service
  #checkov:skip=CKV_AZURE_17:Ensure the web app has 'Client Certificates (Incoming client certificates)' set
  name                          = "staging1"
  app_service_id                = azurerm_linux_web_app.polaris_proxy.id
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_proxy_subnet.id
  public_network_access_enabled = false
  https_only                    = true
  tags                          = local.common_tags

  app_settings = {
    "HostType"                                        = "Staging1"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_SCHEME"                                  = "https"
    "APPINSIGHTS_INSTRUMENTATIONKEY"                  = data.azurerm_application_insights.global_ai.instrumentation_key
    "APPINSIGHTS_PROFILERFEATURE_VERSION"             = "1.0.0"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"             = "1.0.0"
    "APPLICATIONINSIGHTS_CONFIGURATION_CONTENT"       = ""
    "APPLICATIONINSIGHTS_CONNECTION_STRING"           = data.azurerm_application_insights.global_ai.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION"      = "~3"
    "DiagnosticServices_EXTENSION_VERSION"            = "~3"
    "InstrumentationEngine_EXTENSION_VERSION"         = "disabled"
    "SnapshotDebugger_EXTENSION_VERSION"              = "disabled"
    "XDT_MicrosoftApplicationInsights_BaseExtensions" = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"           = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"     = "disabled"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.polaris_sacpspolaris_proxy_staging1_file_share.name
    "DEFAULT_UPSTREAM_CMS_IP_CORSHAM"                 = var.cms_details.default_upstream_cms_ip_corsham
    "DEFAULT_UPSTREAM_CMS_MODERN_IP_CORSHAM"          = var.cms_details.default_upstream_cms_modern_ip_corsham
    "DEFAULT_UPSTREAM_CMS_IP_FARNBOROUGH"             = var.cms_details.default_upstream_cms_ip_farnborough
    "DEFAULT_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH"      = var.cms_details.default_upstream_cms_modern_ip_farnborough
    "DEFAULT_UPSTREAM_CMS_DOMAIN_NAME"                = var.cms_details.default_upstream_cms_domain_name
    "DEFAULT_UPSTREAM_CMS_SERVICES_DOMAIN_NAME"       = var.cms_details.default_upstream_cms_services_domain_name
    "DEFAULT_UPSTREAM_CMS_MODERN_DOMAIN_NAME"         = var.cms_details.default_upstream_cms_modern_domain_name
    "CIN2_UPSTREAM_CMS_IP_CORSHAM"                    = var.cms_details.cin2_upstream_cms_ip_corsham
    "CIN2_UPSTREAM_CMS_MODERN_IP_CORSHAM"             = var.cms_details.cin2_upstream_cms_modern_ip_corsham
    "CIN2_UPSTREAM_CMS_IP_FARNBOROUGH"                = var.cms_details.cin2_upstream_cms_ip_farnborough
    "CIN2_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH"         = var.cms_details.cin2_upstream_cms_modern_ip_farnborough
    "CIN2_UPSTREAM_CMS_DOMAIN_NAME"                   = var.cms_details.cin2_upstream_cms_domain_name
    "CIN2_UPSTREAM_CMS_SERVICES_DOMAIN_NAME"          = var.cms_details.cin2_upstream_cms_services_domain_name
    "CIN2_UPSTREAM_CMS_MODERN_DOMAIN_NAME"            = var.cms_details.cin2_upstream_cms_modern_domain_name
    "CIN4_UPSTREAM_CMS_IP_CORSHAM"                    = var.cms_details.cin4_upstream_cms_ip_corsham
    "CIN4_UPSTREAM_CMS_MODERN_IP_CORSHAM"             = var.cms_details.cin4_upstream_cms_modern_ip_corsham
    "CIN4_UPSTREAM_CMS_IP_FARNBOROUGH"                = var.cms_details.cin4_upstream_cms_ip_farnborough
    "CIN4_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH"         = var.cms_details.cin4_upstream_cms_modern_ip_farnborough
    "CIN4_UPSTREAM_CMS_DOMAIN_NAME"                   = var.cms_details.cin4_upstream_cms_domain_name
    "CIN4_UPSTREAM_CMS_SERVICES_DOMAIN_NAME"          = var.cms_details.cin4_upstream_cms_services_domain_name
    "CIN4_UPSTREAM_CMS_MODERN_DOMAIN_NAME"            = var.cms_details.cin4_upstream_cms_modern_domain_name
    "CIN5_UPSTREAM_CMS_IP_CORSHAM"                    = var.cms_details.cin5_upstream_cms_ip_corsham
    "CIN5_UPSTREAM_CMS_MODERN_IP_CORSHAM"             = var.cms_details.cin5_upstream_cms_modern_ip_corsham
    "CIN5_UPSTREAM_CMS_IP_FARNBOROUGH"                = var.cms_details.cin5_upstream_cms_ip_farnborough
    "CIN5_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH"         = var.cms_details.cin5_upstream_cms_modern_ip_farnborough
    "CIN5_UPSTREAM_CMS_DOMAIN_NAME"                   = var.cms_details.cin5_upstream_cms_domain_name
    "CIN5_UPSTREAM_CMS_SERVICES_DOMAIN_NAME"          = var.cms_details.cin5_upstream_cms_services_domain_name
    "CIN5_UPSTREAM_CMS_MODERN_DOMAIN_NAME"            = var.cms_details.cin5_upstream_cms_modern_domain_name
    "APP_ENDPOINT_DOMAIN_NAME"                        = "${azurerm_linux_web_app.as_web_polaris.name}.azurewebsites.net"
    "APP_SUBFOLDER_PATH"                              = var.polaris_ui_sub_folder
    "API_ENDPOINT_DOMAIN_NAME"                        = "${azurerm_linux_function_app.fa_polaris.name}.azurewebsites.net"
    "AUTH_HANDOVER_ENDPOINT_DOMAIN_NAME"              = "fa-${local.ddei_resource_name}.azurewebsites.net"
    "DDEI_ENDPOINT_DOMAIN_NAME"                       = "fa-${local.ddei_resource_name}.azurewebsites.net"
    "DDEI_ENDPOINT_FUNCTION_APP_KEY"                  = data.azurerm_function_app_host_keys.fa_ddei_host_keys.default_function_key
    "SAS_URL_DOMAIN_NAME"                             = "${azurerm_storage_account.sa.name}.blob.core.windows.net"
    "ENDPOINT_HTTP_PROTOCOL"                          = "https"
    "NGINX_ENVSUBST_OUTPUT_DIR"                       = "/etc/nginx"
    "FORCE_REFRESH_CONFIG"                            = "${md5(file("nginx.conf"))}:${md5(file("nginx.js"))}:${md5(file("cmsenv.js"))}::${md5(file("polaris-script.js"))}"
    "CMS_RATE_LIMIT_QUEUE"                            = "100000000000000000"
    "CMS_RATE_LIMIT"                                  = "128r/s"
    "WM_TASK_LIST_HOST_NAME"                          = var.wm_task_list_host_name
    "AUTH_HANDOVER_WHITELIST"                         = var.auth_handover_whitelist
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS"             = "10"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/"
    "WEBSITE_SWAP_WARMUP_PING_STATUSES"               = "200,202"
    "WEBSITE_WARMUP_PATH"                             = "/"
  }

  site_config {
    ftps_state    = "FtpsOnly"
    http2_enabled = true
    application_stack {
      docker_image_name        = "nginx:latest"
      docker_registry_url      = "https://${data.azurerm_container_registry.polaris_container_registry.login_server}"
      docker_registry_username = data.azurerm_container_registry.polaris_container_registry.admin_username
      docker_registry_password = data.azurerm_container_registry.polaris_container_registry.admin_username
    }
    always_on                               = true
    vnet_route_all_enabled                  = true
    container_registry_use_managed_identity = true
    health_check_path                       = "/"
    health_check_eviction_time_in_min       = "2"
  }

  auth_settings_v2 {
    auth_enabled           = false
    unauthenticated_action = "AllowAnonymous"
    default_provider       = "AzureActiveDirectory"
    excluded_paths         = ["/status"]

    active_directory_v2 {
      tenant_auth_endpoint = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      #checkov:skip=CKV_SECRET_6:Base64 High Entropy String - Misunderstanding of setting "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = module.azurerm_app_reg_polaris_proxy.client_id
    }

    login {
      token_store_enabled = false
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

  logs {
    detailed_error_messages = true
    failed_request_tracing  = true
    http_logs {
      file_system {
        retention_in_days = 3
        retention_in_mb   = 35
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_role_assignment" "ra_blob_data_contributor_polaris_proxy_staging1" {
  scope                = azurerm_storage_container.polaris_proxy_content.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_web_app_slot.polaris_proxy_staging1.identity[0].principal_id
  depends_on           = [azurerm_storage_account.sacpspolaris, azurerm_storage_container.polaris_proxy_content]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_proxy_staging1_pe" {
  name                = "${azurerm_linux_web_app.polaris_proxy.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_web_app.polaris_proxy.name}-staging1-psc"
    private_connection_resource_id = azurerm_linux_web_app.polaris_proxy.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}

resource "azurerm_monitor_diagnostic_setting" "proxy_staging1_diagnostic_settings" {
  name                           = "proxy-staging-diagnostic-settings"
  target_resource_id             = azurerm_linux_web_app_slot.polaris_proxy_staging1.id
  log_analytics_workspace_id     = data.azurerm_log_analytics_workspace.global_la.id
  log_analytics_destination_type = "Dedicated"

  enabled_log {
    category = "AppServiceConsoleLogs"
  }

  depends_on = [azurerm_linux_web_app_slot.polaris_proxy_staging1]
}