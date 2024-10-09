#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_pipeline_ep_pdf_generator" {
  #checkov:skip=CKV_AZURE_212:Ensure App Service has a minimum number of instances for fail over
  #checkov:skip=CKV_AZURE_225:Ensure the App Service Plan is zone redundant
  name                         = "asp-pdf-generator-ep-${local.pipeline_resource_name}"
  location                     = azurerm_resource_group.rg_polaris_pipeline.location
  resource_group_name          = azurerm_resource_group.rg_polaris_pipeline.name
  os_type                      = "Windows"
  sku_name                     = var.pipeline_component_service_plans.pdf_generator_service_plan_sku
  tags                         = local.common_tags
  maximum_elastic_worker_count = var.pipeline_component_service_plans.pdf_generator_plan_maximum_burst
}