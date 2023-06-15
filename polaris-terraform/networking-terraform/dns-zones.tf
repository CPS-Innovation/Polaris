resource "azurerm_private_dns_zone" "dns_zone_blob_storage" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_table_storage" {
  name                = "privatelink.table.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_file_storage" {
  name                = "privatelink.file.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_apps" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_queue_storage" {
  name                = "privatelink.queue.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_key_vault" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_search_service" {
  name                = "privatelink.search.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_cognitive_account" {
  name                = "privatelink.cognitiveservices.azure.com"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_monitor" {
  name                = "privatelink.monitor.azure.com"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_oms" {
  name                = "privatelink.oms.opinsights.azure.com"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_ods" {
  name                = "privatelink.ods.opinsights.azure.com"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}

resource "azurerm_private_dns_zone" "dns_zone_agentsvc" {
  name                = "privatelink.agentsvc.azure-automation.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
  tags                = local.common_tags
}