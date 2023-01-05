#################### Dashboard ####################

resource "azurerm_dashboard" "dashboard_rumpole" {
  name                = "${local.resource_name}-dashboard"
  resource_group_name = azurerm_resource_group.rg_rumpole.name
  location            = azurerm_resource_group.rg_rumpole.location
  dashboard_properties = templatefile("dashboard.tpl",
    {
      sub_id              = data.azurerm_subscription.current.subscription_id,
      resource_group_name = azurerm_resource_group.rg_rumpole.name,
      app_service_name    = azurerm_app_service.as_web_rumpole.name
      function_name       = azurerm_linux_function_app.fa_rumpole.name
      app_insights_name   = azurerm_application_insights.ai_rumpole.name
  })
}
