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
    fqdns = ["${local.resource_name}-cmsproxy.azurewebsites.net"]
    name  = "polaris-app-gateway${local.resource_suffix}-pool"
  }
  backend_http_settings {
    affinity_cookie_name  = "ApplicationGatewayAffinity"
    cookie_based_affinity = "Enabled"
    name                                = "polaris-app-gateway${local.resource_suffix}-proxy-http-settings"
    pick_host_name_from_backend_address = true
    port                                = 80
    probe_name                          = "polaris-app-gateway${local.resource_suffix}-maintenance-http-probe"
    protocol              = "Http"
    request_timeout       = 20
    connection_draining {
      drain_timeout_sec = 60
      enabled           = true
    }
  }
  backend_http_settings {
    affinity_cookie_name  = "ApplicationGatewayAffinity"
    cookie_based_affinity = "Enabled"
    name                                = "polaris-app-gateway${local.resource_suffix}-proxy-https-settings"
    pick_host_name_from_backend_address = true
    port                                = 443
    probe_name                          = "polaris-app-gateway${local.resource_suffix}-maintenance-https-probe"
    protocol                            = "Https"
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
    name                          = "polaris-app-gateway-prip${local.resource_suffix}"
    private_ip_address_allocation = "Static"
    subnet_id                     = data.azurerm_subnet.polaris_app_gateway_subnet.id
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
    subnet_id = data.azurerm_subnet.polaris_app_gateway_subnet.id
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
  probe {
    host                                      = "10.7.196.239"
    interval                                  = 30
    name                                      = "polaris-app-gateway${local.resource_suffix}-maintenance-http-probe"
    path                                      = "/api/status"
    port                                      = 80
    protocol                                  = "Http"
    timeout                                   = 10
    unhealthy_threshold                       = 1
    match {
      status_code = ["200-399"]
    }
  }
  probe {
    host                                      = "10.7.196.239"
    interval                                  = 30
    name                                      = "polaris-app-gateway${local.resource_suffix}-maintenance-https-probe"
    path                                      = "/"
    port                                      = 443
    pick_host_name_from_backend_http_settings = true
    protocol                                  = "Https"
    timeout                                   = 10
    unhealthy_threshold                       = 1
    match {
      status_code = ["200-399"]
    }
  }
  request_routing_rule {
    backend_address_pool_name  = "polaris-app-gateway${local.resource_suffix}-pool"
    backend_http_settings_name = "polaris-app-gateway${local.resource_suffix}-proxy-http-settings"
    http_listener_name         = "polaris-app-gateway${local.resource_suffix}-http-listener"
    name                       = "polaris-app-gateway${local.resource_suffix}-http-rule"
    priority                   = 2
    rule_type                  = "Basic"
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
    key_vault_secret_id = data.azurerm_key_vault_secret.kv_polaris_cert_ssl.id
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

