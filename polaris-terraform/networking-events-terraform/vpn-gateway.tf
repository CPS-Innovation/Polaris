resource "azurerm_public_ip" "polaris_vpn_gateway_pip" {
  allocation_method   = "Static"
  resource_group_name = data.azurerm_resource_group.networking_rg.name
  location            = data.azurerm_resource_group.networking_rg.location
  name                = "polaris-vpn-gateway-pip"
  sku                 = "Standard"
}

resource "azurerm_virtual_network_gateway" "polaris_vpn_gateway" {
  resource_group_name = data.azurerm_resource_group.networking_rg.name
  location            = data.azurerm_resource_group.networking_rg.location
  name                = "polaris-vpn-gateway"
  sku                 = "VpnGw1"
  type                = "Vpn"
  active_active       = false
  enable_bgp          = false
  vpn_type            = "RouteBased"

  ip_configuration {
    name                          = "default"
    public_ip_address_id          = azurerm_public_ip.polaris_vpn_gateway_pip.id
    subnet_id                     = data.azurerm_subnet.sn_gateway_subnet.id
    private_ip_address_allocation = "Dynamic"
  }

  vpn_client_configuration {
    aad_audience  = var.vpn_client_sp_id
    aad_issuer    = "https://sts.windows.net/${data.azurerm_subscription.current.tenant_id}/"
    aad_tenant    = "https://login.microsoftonline.com/${data.azurerm_subscription.current.tenant_id}"
    address_space = [var.vnet_address_space]
  }
  depends_on = [
    azurerm_public_ip.polaris_vpn_gateway_pip
  ]
}

