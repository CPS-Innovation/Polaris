#################### Dashboard ####################

resource "azurerm_dashboard" "dashboard_polaris" {
  name                = "${local.resource_name}-dashboard"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  dashboard_properties = templatefile("dashboard.tpl",
    {
      sub_id              = data.azurerm_subscription.current.subscription_id,
      resource_group_name = azurerm_resource_group.rg_polaris.name,
      app_service_name    = azurerm_linux_web_app.as_web_polaris.name
      function_name       = azurerm_linux_function_app.fa_polaris.name
      app_insights_name   = azurerm_application_insights.ai_polaris.name
  })
  tags = local.common_tags
}
