resource "azurerm_public_ip" "vpn-gateway" {
  count               = var.create_vpn ? 1 : 0
  name                = "vpn-innovation-ip-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  allocation_method   = "Dynamic"
}

resource "azurerm_virtual_network_gateway" "developer_access" {
  count               = var.create_vpn ? 1 : 0
  name                = "vpn-innovation-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name

  type     = "Vpn"
  vpn_type = "RouteBased"
  sku      = "VpnGw1"
  active_active = false
  enable_bgp    = false

  ip_configuration {
    name                          = "vpn-ip-config-${var.environment.name}"
    public_ip_address_id          = azurerm_public_ip.vpn-gateway[count.index].id
    private_ip_address_allocation = "Dynamic"
    subnet_id                     = azurerm_subnet.sn_gateway_subnet.id
  }

  # Only allow connection via Azure AD (not certs). User/group access should be assigned to the provided Azure AD application (audience)
  vpn_client_configuration {
    address_space        = var.vpn_client_ip_pool
    vpn_client_protocols = ["OpenVPN"]
    aad_tenant           = "https://login.microsoftonline.com/${var.vpn_aad_tenant_id}"
    aad_audience         = var.vpn_aad_audience_id
    aad_issuer           = "https://sts.windows.net/${var.vpn_aad_tenant_id}/"
  }

  # VPN creation can take upwards of 30 minutes. Prevent timeout.
  timeouts {
    create = "90m"
  }
}

