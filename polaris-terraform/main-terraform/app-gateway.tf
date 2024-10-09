resource "azurerm_user_assigned_identity" "polaris_app_gateway_identity" {
  count = var.env == "dev" ? 1 : 0

  location            = azurerm_resource_group.rg_polaris.location
  name                = "polaris-app-gateway-identity${local.resource_suffix}"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  tags                = local.common_tags

  depends_on = [azurerm_resource_group.rg_polaris]
}

resource "azurerm_web_application_firewall_policy" "polaris_app_gateway_waf_policy" {
  count = var.env == "dev" ? 1 : 0

  location            = azurerm_resource_group.rg_polaris.location
  name                = "polaris-app-gateway-waf-policy${local.resource_suffix}"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  managed_rules {
    managed_rule_set {
      version = "3.2"
    }
  }
  policy_settings {
    enabled = true
    mode    = "Detection"
  }
  depends_on = [azurerm_resource_group.rg_polaris]
}

resource "azurerm_public_ip" "polaris_app_gateway_public_ip" {
  count = var.env == "dev" ? 1 : 0

  allocation_method   = "Static"
  domain_name_label   = "caseworkapp${local.env_name}"
  location            = azurerm_resource_group.rg_polaris.location
  name                = "polaris-app-gateway-pip${local.resource_suffix}"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  sku                 = "Standard"
  zones               = ["1", "2", "3"]
  depends_on          = [azurerm_resource_group.rg_polaris]
}

resource "azurerm_application_gateway" "polaris_app_gateway" {
  count = var.env == "dev" ? 1 : 0

  enable_http2                      = true
  firewall_policy_id                = azurerm_web_application_firewall_policy.polaris_app_gateway_waf_policy[0].id
  force_firewall_policy_association = true
  location                          = azurerm_resource_group.rg_polaris.location
  name                              = "polaris-app-gateway${local.resource_suffix}"
  resource_group_name               = azurerm_resource_group.rg_polaris.name
  zones                             = ["1", "2", "3"]
  autoscale_configuration {
    max_capacity = 2
    min_capacity = 1
  }
  backend_address_pool {
    fqdns = ["${local.global_resource_name}-cmsproxy.azurewebsites.net"]
    name  = "polaris-app-gateway${local.resource_suffix}-pool"
  }
  backend_http_settings {
    affinity_cookie_name  = "ApplicationGatewayAffinity"
    cookie_based_affinity = "Enabled"
    host_name             = var.app_gateway_back_end_host_name
    name                  = "polaris-app-gateway${local.resource_suffix}-proxy-https-settings"
    port                  = 443
    protocol              = "Https"
    request_timeout       = 20
    connection_draining {
      drain_timeout_sec = 60
      enabled           = true
    }
  }
  frontend_ip_configuration {
    name                 = "appGwPublicFrontendIpIPv4"
    public_ip_address_id = azurerm_public_ip.polaris_app_gateway_public_ip[0].id
  }
  frontend_ip_configuration {
    name                            = "polaris-app-gateway-prip${local.resource_suffix}"
    private_ip_address_allocation   = "Static"
    private_link_configuration_name = "polaris-app-gateway${local.resource_suffix}-plink"
    subnet_id                       = data.azurerm_subnet.polaris_app_gateway_subnet[0].id
  }
  frontend_port {
    name = "port_443"
    port = 443
  }
  frontend_port {
    name = "port_80"
    port = 80
  }
  gateway_ip_configuration {
    name      = "appGatewayIpConfig"
    subnet_id = data.azurerm_subnet.polaris_app_gateway_subnet[0].id
  }
  http_listener {
    frontend_ip_configuration_name = "polaris-app-gateway-prip${local.resource_suffix}"
    frontend_port_name             = "port_443"
    name                           = "polaris-app-gateway${local.resource_suffix}-https-listener"
    protocol                       = "Https"
    ssl_certificate_name           = var.ssl_certificate_name
    custom_error_configuration {
      custom_error_page_url = var.app_gateway_custom_error_pages.HttpStatus502
      status_code           = "HttpStatus502"
    }
    custom_error_configuration {
      custom_error_page_url = var.app_gateway_custom_error_pages.HttpStatus403
      status_code           = "HttpStatus403"
    }
  }
  http_listener {
    frontend_ip_configuration_name = "polaris-app-gateway-prip${local.resource_suffix}"
    frontend_port_name             = "port_80"
    name                           = "polaris-app-gateway${local.resource_suffix}-http-listener"
    protocol                       = "Http"
    custom_error_configuration {
      custom_error_page_url = var.app_gateway_custom_error_pages.HttpStatus502
      status_code           = "HttpStatus502"
    }
    custom_error_configuration {
      custom_error_page_url = var.app_gateway_custom_error_pages.HttpStatus403
      status_code           = "HttpStatus403"
    }
  }
  identity {
    identity_ids = [azurerm_user_assigned_identity.polaris_app_gateway_identity[0].id]
    type         = "UserAssigned"
  }
  private_link_configuration {
    name = "polaris-app-gateway${local.resource_suffix}-plink"
    ip_configuration {
      name                          = "privateLinkIpConfig1"
      primary                       = false
      private_ip_address_allocation = "Dynamic"
      subnet_id                     = data.azurerm_subnet.polaris_apps2_subnet.id
    }
  }
  redirect_configuration {
    include_path         = true
    include_query_string = true
    name                 = "polaris-app-gateway${local.resource_suffix}-http-rule"
    redirect_type        = "Permanent"
    target_listener_name = "polaris-app-gateway${local.resource_suffix}-https-listener"
  }
  request_routing_rule {
    http_listener_name          = "polaris-app-gateway${local.resource_suffix}-http-listener"
    name                        = "polaris-app-gateway${local.resource_suffix}-http-rule"
    priority                    = 2
    redirect_configuration_name = "polaris-app-gateway${local.resource_suffix}-http-rule"
    rule_type                   = "Basic"
  }
  request_routing_rule {
    backend_address_pool_name  = "polaris-app-gateway${local.resource_suffix}-pool"
    backend_http_settings_name = "polaris-app-gateway${local.resource_suffix}-proxy-https-settings"
    http_listener_name         = "polaris-app-gateway${local.resource_suffix}-https-listener"
    name                       = "polaris-app-gateway${local.resource_suffix}-https-rule"
    priority                   = 1
    rule_type                  = "Basic"
  }
  sku {
    name = "WAF_v2"
    tier = "WAF_v2"
  }
  ssl_certificate {
    name                = var.ssl_certificate_name
    key_vault_secret_id = data.azurerm_key_vault_secret.kv_polaris_cert_ssl[0].id
  }
  ssl_profile {
    name = "polaris-app-gateway${local.resource_suffix}-ssl-policy"
    ssl_policy {
      policy_name = var.ssl_policy_name
      policy_type = "Predefined"
    }
  }

  depends_on = [
    azurerm_web_application_firewall_policy.polaris_app_gateway_waf_policy,
    azurerm_public_ip.polaris_app_gateway_public_ip,
    azurerm_user_assigned_identity.polaris_app_gateway_identity,
    azurerm_linux_function_app.fa_polaris_maintenance
  ]
}

resource "azurerm_private_endpoint" "polaris_app_gateway_pe" {
  count = var.env == "dev" ? 1 : 0

  name                = "${azurerm_application_gateway.polaris_app_gateway[0].name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_application_gateway.polaris_app_gateway[0].name}-psc"
    private_connection_resource_id = azurerm_application_gateway.polaris_app_gateway[0].id
    is_manual_connection           = false
    subresource_names              = ["polaris-app-gateway-prip${local.resource_suffix}"]
  }
}