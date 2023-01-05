#################### Application Insights ####################

resource "azurerm_application_insights" "ai_rumpole" {
  name                = "ai-${local.resource_name}"
  location            = azurerm_resource_group.rg_rumpole.location
  resource_group_name = azurerm_resource_group.rg_rumpole.name
  application_type    = "web"
  retention_in_days   = 30
  tags = {
    environment = var.environment_tag
  }
}
