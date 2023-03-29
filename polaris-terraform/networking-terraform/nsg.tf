resource "azurerm_network_security_group" "nsg" {
  location            = azurerm_resource_group.rg_networking.location
  name                = var.nsg_name
  resource_group_name = azurerm_resource_group.rg_networking.name
  depends_on = [
    azurerm_resource_group.rg_networking,
  ]
}

resource "azurerm_network_security_rule" "nsg_rule_1" {
  access                      = "Allow"
  destination_address_prefix  = "*"
  destination_port_range      = "3389"
  direction                   = "Inbound"
  name                        = "AllowAnyCustom3389Inbound"
  network_security_group_name = azurerm_network_security_group.nsg.name
  priority                    = 130
  protocol                    = "*"
  resource_group_name         = azurerm_resource_group.rg_networking.name
  source_address_prefix       = "*"
  source_port_range           = "*"
  depends_on = [
    azurerm_network_security_group.nsg
  ]
}

resource "azurerm_network_security_rule" "nsg_rule_2" {
  access                      = "Allow"
  destination_address_prefix  = "*"
  destination_port_range      = "443"
  direction                   = "Inbound"
  name                        = "AllowAnyCustom443Inbound"
  network_security_group_name = azurerm_network_security_group.nsg.name
  priority                    = 110
  protocol                    = "*"
  resource_group_name         = azurerm_resource_group.rg_networking.name
  source_address_prefix       = "*"
  source_port_range           = "*"
  depends_on = [
    azurerm_network_security_group.nsg
  ]
}

resource "azurerm_network_security_rule" "nsg_rule_3" {
  access                      = "Allow"
  destination_address_prefix  = "*"
  destination_port_range      = "443"
  direction                   = "Outbound"
  name                        = "AllowAnyCustom443Outbound"
  network_security_group_name = azurerm_network_security_group.nsg.name
  priority                    = 101
  protocol                    = "*"
  resource_group_name         = azurerm_resource_group.rg_networking.name
  source_address_prefix       = "*"
  source_port_range           = "*"
  depends_on = [
    azurerm_network_security_group.nsg
  ]
}
resource "azurerm_network_security_rule" "nsg_rule_4" {
  access                      = "Allow"
  destination_address_prefix  = "*"
  destination_port_range      = "80"
  direction                   = "Inbound"
  name                        = "AllowAnyCustom80Inbound"
  network_security_group_name = azurerm_network_security_group.nsg.name
  priority                    = 100
  protocol                    = "*"
  resource_group_name         = azurerm_resource_group.rg_networking.name
  source_address_prefix       = "*"
  source_port_range           = "*"
  depends_on = [
    azurerm_network_security_group.nsg
  ]
}

resource "azurerm_network_security_rule" "nsg_rule_5" {
  access                      = "Allow"
  destination_address_prefix  = "*"
  destination_port_range      = "80"
  direction                   = "Outbound"
  name                        = "AllowAnyCustom80Outbound"
  network_security_group_name = azurerm_network_security_group.nsg.name
  priority                    = 100
  protocol                    = "*"
  resource_group_name         = azurerm_resource_group.rg_networking.name
  source_address_prefix       = "*"
  source_port_range           = "*"
  depends_on = [
    azurerm_network_security_group.nsg
  ]
}
resource "azurerm_network_security_rule" "nsg_rule_6" {
  access                      = "Allow"
  destination_address_prefix  = "*"
  destination_port_range      = "22"
  direction                   = "Inbound"
  name                        = "allow-ssh-from-vpn"
  network_security_group_name = azurerm_network_security_group.nsg.name
  priority                    = 101
  protocol                    = "TCP"
  resource_group_name         = azurerm_resource_group.rg_networking.name
  source_address_prefix       = "*"
  source_port_range           = "*"
  depends_on = [
    azurerm_network_security_group.nsg
  ]
}