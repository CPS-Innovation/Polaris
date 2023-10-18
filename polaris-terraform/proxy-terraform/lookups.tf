data "azuread_client_config" "current" {}

data "azurerm_resource_group" "polaris_resource_group" {
  name     = "rg-${local.resource_name}"
}

data "azurerm_resource_group" "networking_resource_group" {
  name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_resource_group" "analytics_resource_group" {
  name = "rg-${local.analytics_group_name}"
}

data "azurerm_storage_account" "polaris_storage_account" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris"
  resource_group_name = data.azurerm_resource_group.polaris_resource_group.name
}

data "azurerm_storage_account" "pipeline_documents_storage_account" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment_tag}"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_subnet" "polaris_proxy_subnet" {
  name                 = "${var.resource_name_prefix}-proxy-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_subnet" "polaris_apps_subnet" {
  name                 = "${var.resource_name_prefix}-apps-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_application_insights" "global_ai" {
  name                = "ai-${local.resource_name}"
  resource_group_name = data.azurerm_resource_group.analytics_resource_group.name
}

data "azurerm_log_analytics_workspace" "global_la" {
  name                = "la-${local.resource_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_linux_web_app" "polaris_spa" {
  name                = "as-web-${local.resource_name}"
  resource_group_name = data.azurerm_resource_group.polaris_resource_group.name
}

data "azurerm_linux_function_app" "polaris_gateway" {
  name = "fa-${local.resource_name}-gateway"
  resource_group_name = data.azurerm_resource_group.polaris_resource_group.name
}

data "azurerm_linux_function_app" "polaris_auth_handover" {
  name = "fa-${local.resource_name}-auth-handover"
  resource_group_name = data.azurerm_resource_group.polaris_resource_group.name
}

data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}

data "azurerm_container_registry" "polaris_container_registry" {
  name                = "polariscontainers${var.env}"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}

data "azurerm_private_dns_zone" "dns_zone_apps" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = data.azurerm_resource_group.networking_resource_group.name
}