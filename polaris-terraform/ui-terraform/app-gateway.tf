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
    name  = "polaris-app-bep${local.resource_suffix}"
  }
  backend_http_settings {
    affinity_cookie_name  = "ApplicationGatewayAffinity"
    cookie_based_affinity = "Enabled"
    host_name             = "${local.resource_name}-cmsproxy.azurewebsites.net"
    name                  = "polaris-app-gateway-bes${local.resource_suffix}"
    port                  = 443
    probe_name            = "polaris-app-gateway-bes-proxy-probe-1${local.resource_suffix}"
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
    name                           = "polaris-app-gateway-listener-https${local.resource_suffix}"
    protocol                       = "Https"
    ssl_certificate_name           = var.ssl_certificate_name
  }
  http_listener {
    frontend_ip_configuration_name = "polaris-app-gateway-prip${local.resource_suffix}"
    frontend_port_name             = "port_80"
    name                           = "polaris-app-gateway-listener${local.resource_suffix}"
    protocol                       = "Http"
  }
  identity {
    identity_ids = [azurerm_user_assigned_identity.polaris_app_gateway_identity[0].id]
    type         = "UserAssigned"
  }
  probe {
    host                = "${local.resource_name}-cmsproxy.azurewebsites.net"
    interval            = 30
    name                = "polaris-app-gateway-bes-proxy-probe-1${local.resource_suffix}"
    path                = "/"
    protocol            = "Https"
    timeout             = 30
    unhealthy_threshold = 3
    match {
      status_code = ["200-399"]
    }
  }
  probe {
    host                = "${azurerm_linux_function_app.fa_polaris_maintenance.name}.azurewebsites.net"
    interval            = 30
    name                = "polaris-app-gateway-bes-proxy-probe-2${local.resource_suffix}"
    path                = "/"
    protocol            = "Https"
    timeout             = 30
    unhealthy_threshold = 3
    match {
      status_code = ["200-399"]
    }
  }
  request_routing_rule {
    backend_address_pool_name  = "polaris-app-bep${local.resource_suffix}"
    backend_http_settings_name = "polaris-app-gateway-bes${local.resource_suffix}"
    http_listener_name         = "polaris-app-gateway-listener${local.resource_suffix}"
    name                       = "polaris-app-gateway-http-rule${local.resource_suffix}"
    priority                   = 1
    rule_type                  = "Basic"
  }
  request_routing_rule {
    backend_address_pool_name  = "polaris-app-bep${local.resource_suffix}"
    backend_http_settings_name = "polaris-app-gateway-bes${local.resource_suffix}"
    http_listener_name         = "polaris-app-gateway-listener-https${local.resource_suffix}"
    name                       = "polaris-app-gateway-https-rule${local.resource_suffix}"
    priority                   = 2
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

