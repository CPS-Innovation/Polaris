data "azuread_application" "fa_ddei" {
  display_name = "fa-${local.ddei_resource_name}-appreg"
}

data "azuread_service_principal" "fa_ddei_service_principal" {
  application_id = data.azuread_application.fa_ddei.application_id
}

data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment_tag}"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_sa_subnet" {
  name                 = "${var.resource_name_prefix}-sa-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_sa2_subnet" {
  name                 = "${var.resource_name_prefix}-sa2-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_coordinator_subnet" {
  name                 = "${var.resource_name_prefix}-coordinator-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_pdfgenerator_subnet" {
  name                 = "${var.resource_name_prefix}-pdfgenerator-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_textextractor_subnet" {
  name                 = "${var.resource_name_prefix}-textextractor-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_gateway_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-gateway-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_apps_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-apps-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_ci_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-ci-subnet"
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

data "azurerm_private_dns_zone" "dns_zone_apps" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_keyvault" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_search_service" {
  name                = "privatelink.search.windows.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_cognitive_account" {
  name                = "privatelink.cognitiveservices.azure.com"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_event_hub_namespace" {
  name                = "privatelink.servicebus.windows.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_key_vault" "terraform_key_vault" {
  name                = "kv${var.env}terraform"
  resource_group_name = "rg-terraform"
}

data "azurerm_application_insights" "global_ai" {
  name                = "ai-${local.global_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_log_analytics_workspace" "global_la" {
  name                = "la-${local.global_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}