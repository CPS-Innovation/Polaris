#################### Application Insights ####################

resource "azurerm_application_insights" "ai_polaris" {
  name                = "ai-${local.resource_name}"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  application_type    = "web"
  retention_in_days   = 30
  tags                = local.common_tags
}
