# ensuring storage account has been created
resource "azurerm_role_assignment" "gateway_blob_data_contributor" {
  scope                = data.azurerm_storage_container.pipeline_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azurerm_linux_function_app.fa_gateway.identity[0].principal_id
}