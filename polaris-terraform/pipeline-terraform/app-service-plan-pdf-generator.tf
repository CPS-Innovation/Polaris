#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_pipeline_pdf_generator" {
  #checkov:skip=CKV_AZURE_212:Ensure App Service has a minimum number of instances for fail over
  name                         = "asp-pdf-generator-ep-${local.resource_name}"
  location                     = azurerm_resource_group.rg.location
  resource_group_name          = azurerm_resource_group.rg.name
  os_type                      = "Windows"
  sku_name                     = var.pipeline_component_service_plans.pdf_generator_service_plan_sku
  tags                         = local.common_tags
  maximum_elastic_worker_count = var.pipeline_component_service_plans.pdf_generator_maximum_instances
}