#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_rumpole" {
  name                = "asp-${local.resource_name}"
  location            = azurerm_resource_group.rg_rumpole.location
  resource_group_name = azurerm_resource_group.rg_rumpole.name
  os_type             = "Linux"
  sku_name            = var.app_service_plan_sku.size

  tags = {
    environment = var.environment_tag
  }
}

resource "azurerm_monitor_autoscale_setting" "amas_rumpole" {
  name                = "amas-${local.resource_name}"
  count               = var.app_service_plan_sku.tier != "Basic" ? 1 : 0
  resource_group_name = azurerm_resource_group.rg_rumpole.name
  location            = azurerm_resource_group.rg_rumpole.location
  target_resource_id  = azurerm_service_plan.asp_rumpole.id
  profile {
    name = "Rumpole Performance Scaling Profile"
    capacity {
      default = 1
      minimum = 1
      maximum = 10
    }
    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.asp_rumpole.id
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
        metric_resource_id = azurerm_service_plan.asp_rumpole.id
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
