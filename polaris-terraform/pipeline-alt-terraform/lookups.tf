data "azurerm_resource_group" "rg_polaris_pipeline" {
  name = "rg-${local.resource_name}"
}

data "azurerm_service_plan" "pipeline_service_plan" {
  name = "asp-linux-${local.resource_name}"
  resource_group_name = "rg-${local.resource_name}"
}

data "azurerm_storage_account" "pipeline_sa" {
  name = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
  resource_group_name = "rg-${local.resource_name}"
}

data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment_tag}"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_coordinator_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-coordinator-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_pdfgenerator_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-pdfgenerator-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_textextractor_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-textextractor-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_apps_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-apps-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_apps" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_search_service" "pipeline_search_service" {
  name                 = local.search_service_name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}

data "azurerm_application_insights" "global_ai" {
  name                = "ai-${local.global_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_key_vault" "polaris_pipeline_key_vault" {
  name                = "kv-${local.resource_name}"
  resource_group_name = "rg-${local.resource_name}"
}

data "azurerm_key_vault_secret" "pipeline_storage_connection_string" {
  name         = "PipelineStorageConnectionString"
  key_vault_id = data.azurerm_key_vault.polaris_pipeline_key_vault.id
}

data "azurerm_cognitive_account" "pipeline_computer_vision_service" {
  name                = "cv-${local.resource_name}"
  resource_group_name = "rg-${local.resource_name}"
}

data "azurerm_storage_container" "pipeline_storage_container" {
  name                 = "documents"
  storage_account_name = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
}

data "azuread_service_principal" "computer_vision_sp" {
  display_name = "cv-${local.resource_name}"
}