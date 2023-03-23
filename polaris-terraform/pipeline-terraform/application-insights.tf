#################### Application Insights and Log Analytics Workspaces ####################

resource "azurerm_application_insights" "ai" {
  name                = "ai-${local.resource_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
  retention_in_days   = 90
  tags                = local.common_tags
}

resource "azurerm_log_analytics_workspace" "polaris_analytics_workspace" {
  name                = "exampleworkspace"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}
