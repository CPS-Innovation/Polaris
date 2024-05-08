#################### Dashboard ####################

resource "azurerm_portal_dashboard" "dashboard_polaris" {
  name                = "${local.resource_name}-dashboard"
  resource_group_name = data.azurerm_resource_group.rg_analytics.name
  location            = data.azurerm_resource_group.rg_analytics.location
  dashboard_properties = templatefile("dashboard.tpl",
    {
      sub_id              = data.azurerm_subscription.current.subscription_id,
      resource_group_name = data.azurerm_resource_group.rg_analytics.name,
      app_service_name    = azurerm_linux_web_app.as_web_polaris.name
      function_name       = azurerm_linux_function_app.fa_polaris.name
      app_insights_name   = data.azurerm_application_insights.global_ai.name
  })
  tags = local.common_tags
}
