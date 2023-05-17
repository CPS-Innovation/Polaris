resource "azapi_resource" "pipeline_sa_coordinator_alt_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "pipeline-coordinator-alt-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${data.azurerm_resource_group.rg_polaris_pipeline.name}/providers/Microsoft.Storage/storageAccounts/${data.azurerm_storage_account.pipeline_sa.name}/fileServices/default"
}

resource "azapi_resource" "pipeline_sa_pdf_generator_alt_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "pipeline-pdf-generator-alt-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${data.azurerm_resource_group.rg_polaris_pipeline.name}/providers/Microsoft.Storage/storageAccounts/${data.azurerm_storage_account.pipeline_sa.name}/fileServices/default"
}

resource "azapi_resource" "pipeline_sa_text_extractor_alt_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "pipeline-text-extractor-alt-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${data.azurerm_resource_group.rg_polaris_pipeline.name}/providers/Microsoft.Storage/storageAccounts/${data.azurerm_storage_account.pipeline_sa.name}/fileServices/default"
}