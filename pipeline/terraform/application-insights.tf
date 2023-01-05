#################### Application Insights and Log Analytics Workspaces ####################

resource "azurerm_application_insights" "ai" {
  name                = "ai-${local.resource_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
  retention_in_days   = 90
}
