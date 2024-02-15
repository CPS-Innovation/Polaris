#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_proxy" {
  #checkov:skip=CKV_AZURE_212:Ensure App Service has a minimum number of instances for fail over
  #checkov:skip=CKV_AZURE_225:Ensure the App Service Plan is zone redundant
  name                = "asp-${local.resource_name}-proxy"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  os_type             = "Linux"
  sku_name            = var.ui_component_service_plans.proxy_service_plan_sku
  tags                = local.common_tags
}

resource "azurerm_monitor_autoscale_setting" "amas_polaris_proxy" {
  name                = "amas-${local.resource_name}-proxy"
  tags                = local.common_tags
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  target_resource_id  = azurerm_service_plan.asp_polaris_proxy.id
  profile {
    name = "Polaris Proxy Performance Scaling Profile"
    capacity {
      default = 1
      minimum = 1
      maximum = 1
    }
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.asp_polaris_proxy.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 80
      }
      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT1M"
      }
    }
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.asp_polaris_proxy.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "LessThan"
        threshold          = 50
      }
      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT1M"
      }
    }
  }
}