data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}

data "azuread_application" "fa_ddei" {
  display_name        = "fa-${local.ddei_resource_name}-appreg"
}

data "azuread_service_principal" "fa_ddei_service_principal" {
  application_id = data.azuread_application.fa_ddei.application_id
}

data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment_tag}"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_apps_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-app-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_blob_storage" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_table_storage" {
  name                = "privatelink.table.core.windows.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_file_storage" {
  name                = "privatelink.file.core.windows.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_queue_storage" {
  name                = "privatelink.queue.core.windows.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}