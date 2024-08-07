data "azuread_client_config" "current" {}

data "azuread_application" "fa_pipeline_coordinator" {
  display_name = "fa-${local.resource_name}-coordinator-appreg"
}

data "azuread_application" "fa_pipeline_pdf_generator" {
  display_name = "fa-${local.resource_name}-pdf-generator-appreg"
}

data "azuread_application" "fa_ddei" {
  display_name = "fa-${local.ddei_resource_name}-appreg"
}

data "azurerm_search_service" "pipeline_ss" {
  name                = "ss-${local.pipeline_resource_name}"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_storage_account" "sacpspolarispipeline" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azuread_service_principal" "fa_pipeline_coordinator_service_principal" {
  client_id = data.azuread_application.fa_pipeline_coordinator.client_id
}

data "azuread_service_principal" "fa_pdf_generator_service_principal" {
  client_id = data.azuread_application.fa_pipeline_pdf_generator.client_id
}

data "azuread_service_principal" "fa_ddei_service_principal" {
  client_id = data.azuread_application.fa_ddei.client_id
}

data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment_tag}"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_gateway_subnet" {
  name                 = "${var.resource_name_prefix}-gateway-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_ui_subnet" {
  name                 = "${var.resource_name_prefix}-ui-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_proxy_subnet" {
  name                 = "${var.resource_name_prefix}-proxy-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_apps_subnet" {
  name                 = "${var.resource_name_prefix}-apps-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_apps2_subnet" {
  name                 = "${var.resource_name_prefix}-apps2-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_ci_subnet" {
  name                 = "${var.resource_name_prefix}-ci-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_app_gateway_subnet" {
  count = var.env == "dev" ? 1 : 0

  name                 = "${var.resource_name_prefix}-app-gateway-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_maintenance_subnet" {
  count = var.env == "dev" ? 1 : 0

  name                 = "${var.resource_name_prefix}-maintenance-subnet"
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

data "azurerm_container_registry" "polaris_container_registry" {
  name                = "polariscontainers${var.env}"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_application_insights" "global_ai" {
  name                = "ai-${local.resource_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_log_analytics_workspace" "global_la" {
  name                = "la-${local.resource_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_resource_group" "rg_analytics" {
  name = "rg-${local.analytics_group_name}"
}

data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}

data "azuread_application" "fa_redaction_log_reporting" {
  display_name = "fa-${local.redaction_log_resource_name}-reporting"
}

data "azurerm_key_vault" "kv_polaris_cert" {
  count = var.env == "dev" ? 1 : 0

  name                = local.app_service_certificate_store
  resource_group_name = "rg-${local.resource_name}"
}

data "azurerm_key_vault_secret" "kv_polaris_cert_ssl" {
  count = var.env == "dev" ? 1 : 0

  name         = var.ssl_certificate_name
  key_vault_id = data.azurerm_key_vault.kv_polaris_cert[0].id
}