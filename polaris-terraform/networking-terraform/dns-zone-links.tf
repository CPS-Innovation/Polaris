resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_blob_storage_link" {
  name                  = "dnszonelink-blobstorage"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_blob_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id

  depends_on = [azurerm_virtual_network.vnet_networking, azurerm_private_dns_zone.dns_zone_blob_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_table_storage_link" {
  name                  = "dnszonelink-tablestorage"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_table_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id

  depends_on = [azurerm_virtual_network.vnet_networking, azurerm_private_dns_zone.dns_zone_table_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_file_storage_link" {
  name                  = "dnszonelink-filestorage"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_file_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id

  depends_on = [azurerm_virtual_network.vnet_networking, azurerm_private_dns_zone.dns_zone_file_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_apps_link" {
  name                  = "dnszonelink-apps"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_apps.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id

  depends_on = [azurerm_virtual_network.vnet_networking, azurerm_private_dns_zone.dns_zone_apps]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_queue_link" {
  name                  = "dnszonelink-queue"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_queue_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id

  depends_on = [azurerm_virtual_network.vnet_networking, azurerm_private_dns_zone.dns_zone_queue_storage]
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_keyvault_link" {
  name                  = "dnszonelink-keyvault"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_key_vault.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id

  depends_on = [azurerm_virtual_network.vnet_networking, azurerm_private_dns_zone.dns_zone_key_vault]
}