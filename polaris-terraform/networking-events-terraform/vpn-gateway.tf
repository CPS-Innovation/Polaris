resource "azurerm_public_ip" "polaris_vpn_gateway_pip" {
  allocation_method   = "Static"
  resource_group_name = azurerm_resource_group.rg_networking.name
  location            = azurerm_resource_group.rg_networking.location
  name                = "polaris-vpn-gateway-pip"
  sku                 = "Standard"
  depends_on = [
    azurerm_resource_group.rg_networking
  ]
}

resource "azurerm_virtual_network_gateway" "res-95" {
  resource_group_name = azurerm_resource_group.rg_networking.name
  location            = azurerm_resource_group.rg_networking.location
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
    root_certificate {
      name             = var.gateway_certificate_details.root_name
      public_cert_data = var.gateway_certificate_details.public_cert_data
    }
  }
  depends_on = [
    azurerm_public_ip.polaris_vpn_gateway_pip
  ]
}

