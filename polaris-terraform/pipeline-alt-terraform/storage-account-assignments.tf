resource "azurerm_role_assignment" "ra_blob_delegator_text_extractor_alt" {
  scope                = data.azurerm_storage_account.pipeline_sa.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_linux_function_app.fa_text_extractor_alt.identity[0].principal_id
  
  depends_on = [azurerm_linux_function_app.fa_text_extractor_alt]
}

resource "azurerm_role_assignment" "ra_blob_delegator_coordinator" {
  scope                = data.azurerm_storage_account.pipeline_sa.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_linux_function_app.fa_coordinator_alt.identity[0].principal_id

  depends_on = [azurerm_linux_function_app.fa_coordinator_alt]
}

resource "azurerm_role_assignment" "ra_blob_data_contributor_pdf_generator" {
  scope                = data.azurerm_storage_container.pipeline_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_windows_function_app.fa_pdf_generator_alt.identity[0].principal_id

  depends_on = [azurerm_windows_function_app.fa_pdf_generator_alt]
}

resource "azurerm_role_assignment" "ra_blob_data_contributor_text_extractor" {
  scope                = data.azurerm_storage_container.pipeline_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_function_app.fa_text_extractor_alt.identity[0].principal_id

  depends_on = [azurerm_linux_function_app.fa_text_extractor_alt]
}

resource "azurerm_role_assignment" "ra_blob_data_reader_computer_vision_account" {
  scope                = data.azurerm_storage_container.pipeline_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azuread_service_principal.computer_vision_sp.object_id
}

resource "azurerm_role_assignment" "coordinator_blob_data_contributor" {
  scope                = data.azurerm_storage_container.pipeline_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_function_app.fa_coordinator_alt.identity[0].principal_id

  depends_on = [azurerm_linux_function_app.fa_coordinator_alt]
}