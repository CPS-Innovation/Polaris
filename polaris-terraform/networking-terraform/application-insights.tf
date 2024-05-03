#################### Application Insights ####################
resource "azurerm_log_analytics_workspace" "ai_polaris_workspace" {
  name                       = "la-${local.global_name}"
  location                   = azurerm_resource_group.rg_polaris_workspace.location
  resource_group_name        = azurerm_resource_group.rg_polaris_workspace.name
  sku                        = "PerGB2018"
  retention_in_days          = var.insights_configuration.log_retention_days
  internet_ingestion_enabled = var.insights_configuration.analytics_internet_ingestion_enabled
  internet_query_enabled     = var.insights_configuration.analytics_internet_query_enabled
}

resource "azurerm_application_insights" "ai_polaris" {
  name                       = "ai-${local.global_name}"
  location                   = azurerm_resource_group.rg_polaris_workspace.location
  resource_group_name        = azurerm_resource_group.rg_polaris_workspace.name
  workspace_id               = azurerm_log_analytics_workspace.ai_polaris_workspace.id
  application_type           = "web"
  retention_in_days          = var.insights_configuration.log_retention_days
  tags                       = local.common_tags
  internet_ingestion_enabled = var.insights_configuration.insights_internet_ingestion_enabled
  internet_query_enabled     = var.insights_configuration.insights_internet_query_enabled
}

resource "azurerm_monitor_private_link_scope" "pls_ai_insights" {
  name                = "pls-ai-insights"
  resource_group_name = "rg-${var.resource_name_prefix}"
  tags                = local.common_tags
}

resource "azurerm_monitor_private_link_scoped_service" "pls_ai_insights_service" {
  name                = "ai-${local.global_name}-scoped-service"
  resource_group_name = "rg-${var.resource_name_prefix}"
  scope_name          = azurerm_monitor_private_link_scope.pls_ai_insights.name
  linked_resource_id  = azurerm_application_insights.ai_polaris.id

  depends_on = [azurerm_application_insights.ai_polaris]
}

resource "azurerm_monitor_private_link_scoped_service" "pls_ai_workspace_service" {
  name                = "la-${local.global_name}-scoped-service"
  resource_group_name = "rg-${var.resource_name_prefix}"
  scope_name          = azurerm_monitor_private_link_scope.pls_ai_insights.name
  linked_resource_id  = azurerm_log_analytics_workspace.ai_polaris_workspace.id

  depends_on = [azurerm_log_analytics_workspace.ai_polaris_workspace]
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_ampls_pe" {
  name                = "${azurerm_monitor_private_link_scope.pls_ai_insights.name}-pe"
  resource_group_name = "rg-${var.resource_name_prefix}"
  location            = data.azurerm_resource_group.rg_networking.location
  subnet_id           = azurerm_subnet.sn_polaris_ampls_subnet.id

  private_dns_zone_group {
    name = "ampls"
    private_dns_zone_ids = [
      azurerm_private_dns_zone.dns_zone_monitor.id,
      azurerm_private_dns_zone.dns_zone_oms.id,
      azurerm_private_dns_zone.dns_zone_ods.id,
      azurerm_private_dns_zone.dns_zone_agentsvc.id,
      azurerm_private_dns_zone.dns_zone_blob_storage.id
    ]
  }

  private_service_connection {
    name                           = "${azurerm_monitor_private_link_scope.pls_ai_insights.name}-psc"
    private_connection_resource_id = azurerm_monitor_private_link_scope.pls_ai_insights.id
    is_manual_connection           = false
    subresource_names              = ["azuremonitor"]
  }

  tags = local.common_tags
}

# Create DNS A Records for AMPLS
resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_monitor_api" {
  name                = "polaris-ampls-dns-monitor-api"
  zone_name           = azurerm_private_dns_zone.dns_zone_monitor.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 7)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_monitor_global" {
  name                = "polaris-ampls-dns-monitor-global"
  zone_name           = azurerm_private_dns_zone.dns_zone_monitor.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 8)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_monitor_profiler" {
  name                = "polaris-ampls-monitor-dns-profiler"
  zone_name           = azurerm_private_dns_zone.dns_zone_monitor.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 9)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_monitor_live" {
  name                = "polaris-ampls-monitor-dns-live"
  zone_name           = azurerm_private_dns_zone.dns_zone_monitor.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 10)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_monitor_snapshot" {
  name                = "polaris-ampls-monitor-dns-snapshot"
  zone_name           = azurerm_private_dns_zone.dns_zone_monitor.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 11)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_oms_law" {
  name                = azurerm_log_analytics_workspace.ai_polaris_workspace.workspace_id
  zone_name           = azurerm_private_dns_zone.dns_zone_oms.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 4)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_ods_law" {
  name                = azurerm_log_analytics_workspace.ai_polaris_workspace.workspace_id
  zone_name           = azurerm_private_dns_zone.dns_zone_ods.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 5)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_agentsvc_law" {
  name                = azurerm_log_analytics_workspace.ai_polaris_workspace.workspace_id
  zone_name           = azurerm_private_dns_zone.dns_zone_agentsvc.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 6)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_blob_storage" {
  name                = "polaris-ampls-monitor-dns-blob"
  zone_name           = azurerm_private_dns_zone.dns_zone_blob_storage.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 12)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_oms_ai" {
  name                = azurerm_application_insights.ai_polaris.app_id
  zone_name           = azurerm_private_dns_zone.dns_zone_oms.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 13)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_ods_ai" {
  name                = azurerm_application_insights.ai_polaris.app_id
  zone_name           = azurerm_private_dns_zone.dns_zone_ods.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 14)]
  tags                = local.common_tags
}

resource "azurerm_private_dns_a_record" "polaris_ampls_dns_a_agentsvc_ai" {
  name                = azurerm_application_insights.ai_polaris.app_id
  zone_name           = azurerm_private_dns_zone.dns_zone_agentsvc.name
  resource_group_name = "rg-${var.resource_name_prefix}"
  ttl                 = 3600
  records             = [cidrhost(azurerm_subnet.sn_polaris_ampls_subnet.address_prefixes[0], 15)]
  tags                = local.common_tags
}