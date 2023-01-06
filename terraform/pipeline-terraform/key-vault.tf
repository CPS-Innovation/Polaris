#################### Key Vault ####################

resource "azurerm_key_vault" "kv" {
  name                = "kv-${local.resource_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  enabled_for_deployment          = true
  enabled_for_template_deployment = true
  enabled_for_disk_encryption     = true
  enable_rbac_authorization       = true
  purge_protection_enabled        = false
  soft_delete_retention_days      = 90

  sku_name = "standard"
}

resource "azurerm_key_vault_key" "kvap_sa_customer_managed_key" {
  name         = "tfex-key"
  key_vault_id = azurerm_key_vault.kv.id
  key_type     = "RSA"
  key_size     = 2048
  key_opts     = ["decrypt", "encrypt", "sign", "unwrapKey", "verify", "wrapKey"]
  expiration_date = timeadd(timestamp(), "8760h")

  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azurerm_role_assignment.kv_role_sa_kvcseu
  ]
}

resource "azurerm_key_vault_secret" "kvs_fa_coordinator_client_secret" {
  name         = "CoordinatorFunctionAppRegistrationClientSecret"
  value        = azuread_application_password.faap_fa_coordinator_app_service.value
  key_vault_id = azurerm_key_vault.kv.id
  expiration_date = timeadd(timestamp(), "8760h")
  content_type = "password"

  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azurerm_role_assignment.kv_role_sa_kvcseu
  ]
}

resource "azurerm_role_assignment" "kv_role_terraform_sp" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
}

resource "azurerm_role_assignment" "kv_role_sa_kvcseu" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  principal_id         = azurerm_storage_account.sa.identity.0.principal_id
  depends_on           = [azurerm_storage_account.sa]
}

resource "azurerm_role_assignment" "kv_role_fa_coordinator_crypto_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_function_app.fa_coordinator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_coordinator_secrets_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_function_app.fa_coordinator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_pdf_generator_crypto_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_function_app.fa_pdf_generator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_pdf_generator_secrets_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_function_app.fa_pdf_generator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_text_extractor_crypto_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_function_app.fa_text_extractor.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_text_extractor_secrets_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_function_app.fa_text_extractor.identity[0].principal_id
}
