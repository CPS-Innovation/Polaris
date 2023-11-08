#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_pipeline_ep_coordinator" {
  #checkov:skip=CKV_AZURE_212:Ensure App Service has a minimum number of instances for fail over
  #checkov:skip=CKV_AZURE_225:Ensure the App Service Plan is zone redundant
  name                         = "asp-coordinator-ep-${local.resource_name}"
  location                     = azurerm_resource_group.rg.location
  resource_group_name          = azurerm_resource_group.rg.name
  os_type                      = "Linux"
  sku_name                     = var.pipeline_component_service_plans.coordinator_service_plan_sku
  tags                         = local.common_tags
  maximum_elastic_worker_count = var.pipeline_component_service_plans.coordinator_plan_maximum_burst
}

resource "azurerm_service_plan" "asp_polaris_pipeline_ep_coordinator_v2" {
  #checkov:skip=CKV_AZURE_212:Ensure App Service has a minimum number of instances for fail over
  #checkov:skip=CKV_AZURE_225:Ensure the App Service Plan is zone redundant
  name                         = "asp-coordinator-v2-ep-${local.resource_name}"
  location                     = azurerm_resource_group.rg.location
  resource_group_name          = azurerm_resource_group.rg.name
  os_type                      = "Linux"
  sku_name                     = var.pipeline_component_service_plans.coordinator_service_plan_sku
  tags                         = local.common_tags
  maximum_elastic_worker_count = var.pipeline_component_service_plans.coordinator_plan_maximum_burst
}