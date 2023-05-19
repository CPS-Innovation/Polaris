#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_pipeline_coordinator" {
  name                = "asp-coordinator-${local.resource_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = var.coordinator_service_plan_sku
  tags                = local.common_tags
}

resource "azurerm_monitor_autoscale_setting" "amas_polaris_pipeline_coordinator" {
  name                = "amas-${local.resource_name}"
  tags                = local.common_tags
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  target_resource_id  = azurerm_service_plan.asp_polaris_pipeline_coordinator.id
  enabled             = false 
  profile {
    name = "Polaris Pipeline Coordinator App Service Performance Scaling Profile"
    capacity {
      default = 1
      minimum = 1
      maximum = 1
    }
  }
}