resource "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  address_space       = [var.vnetAddressSpace]
  
  tags = {
    environment = var.environment.name
  }
}

data "azurerm_virtual_hub" "digital_platform_virtual_hub" {
  provider            = azurerm.digital-platform-shared
  name                = "digital-platform-virtual-hub"
  resource_group_name = "digital-platform-virtual-hub"
}

/*
resource "azurerm_virtual_hub_connection" "vhc_innovation" {
  provider                  = azurerm.digital-platform-shared
  name                      = "vnet-innovation-${var.environment.name}"
  virtual_hub_id            = data.azurerm_virtual_hub.digital_platform_virtual_hub.id
  remote_virtual_network_id = azurerm_virtual_network.vnet_networking.id
  internet_security_enabled = true


  routing {
    // This is not ideal. Virtual hub route tables are not currently supported as data attributes. We could do this using Azure CLI in the future as a workaround.
    associated_route_table_id = "/subscriptions/8eeb7cbd-fa86-46be-9112-c72428713fc8/resourceGroups/digital-platform-virtual-hub/providers/Microsoft.Network/virtualHubs/digital-platform-virtual-hub/hubRouteTables/defaultRouteTable"
    propagated_route_table {
      labels          = ["default"]
      route_table_ids = ["/subscriptions/8eeb7cbd-fa86-46be-9112-c72428713fc8/resourceGroups/digital-platform-virtual-hub/providers/Microsoft.Network/virtualHubs/digital-platform-virtual-hub/hubRouteTables/defaultRouteTable"]
    }
  }

  depends_on = [
    azurerm_virtual_network.vnet_networking
  ]
}*/