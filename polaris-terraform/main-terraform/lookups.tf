# begin: global config lookup
data "azuread_client_config" "current" {}
# end: global config lookup

# begin: ddei lookup
data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}
# end: ddei lookup

# begin: MDS FA secret lookup - Retrieval of secret stored in vault for the host keys of the MDS FA which is in a different subscription. Secret is a manual entry.
data "azurerm_key_vault_secret" "kvs_fa_mds_host_keys" {
  name         = "MDS-FA-AccessKey"
  key_vault_id = azurerm_key_vault.kv_polaris.id
}
# end: MDS FA secret lookup

# begin: vnet lookup
data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment_tag}"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

# begin: vnet subnet lookups
data "azurerm_subnet" "polaris_sa_subnet" {
  name                 = "${var.pipeline_resource_name_prefix}-sa-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_sa2_subnet" {
  name                 = "${var.pipeline_resource_name_prefix}-sa2-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
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

data "azurerm_subnet" "polaris_coordinator_subnet" {
  name                 = "${var.pipeline_resource_name_prefix}-coordinator-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_pdfgenerator_subnet" {
  name                 = "${var.pipeline_resource_name_prefix}-pdfgenerator-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_pdfthumbnailgenerator_subnet" {
  name                 = "${var.pipeline_resource_name_prefix}-pdfthumbnailgenerator-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_pdfredactor_subnet" {
  name                 = "${var.pipeline_resource_name_prefix}-pdfredactor-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_textextractor_2_subnet" {
  name                 = "${var.pipeline_resource_name_prefix}-textextractor-subnet-2"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_subnet" "polaris_maintenance_subnet" {
  count = var.env == "dev" ? 1 : 0

  name                 = "${var.resource_name_prefix}-maintenance-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_suffix}"
}
# end: vnet subnet lookups

# begin: vnet dns zone lookups
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

data "azurerm_private_dns_zone" "dns_zone_cognitive_account" {
  name                = "privatelink.cognitiveservices.azure.com"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_private_dns_zone" "dns_zone_search_service" {
  name                = "privatelink.search.windows.net"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}
# end: vnet dns zone lookups
# end: vnet lookup

# begin: app insights lookups
data "azurerm_application_insights" "global_ai" {
  name                = "ai-${local.global_resource_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_log_analytics_workspace" "global_la" {
  name                = "la-${local.global_resource_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_resource_group" "rg_analytics" {
  name = "rg-${local.analytics_group_name}"
}
# end: app insights lookups

# begin: external element lookups
data "azurerm_container_registry" "polaris_container_registry" {
  name                = "polariscontainers${var.env}"
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
}

data "azurerm_key_vault" "terraform_key_vault" {
  name                = "kv${var.env}terraform"
  resource_group_name = "rg-terraform"
}

data "azuread_application" "fa_redaction_log_reporting" {
  display_name = "fa-${local.redaction_log_resource_name}-reporting"
}
# end: external element lookups