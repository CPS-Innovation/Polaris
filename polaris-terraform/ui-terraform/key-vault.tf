#################### Key Vault ####################

resource "azurerm_key_vault" "kv_polaris" {
  name                = "kv-${local.resource_name}"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  sku_name = "standard"
}

resource "azurerm_key_vault_access_policy" "kvap_fa_polaris_gateway" {
  key_vault_id = azurerm_key_vault.kv_polaris.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_function_app.fa_polaris.identity[0].principal_id

  secret_permissions = [
    "Get",
  ]
}

resource "azurerm_key_vault_access_policy" "kvap_terraform_sp" {
  key_vault_id = azurerm_key_vault.kv_polaris.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azuread_service_principal.terraform_service_principal.object_id

  secret_permissions = [
    "Get",
    "Set",
    "Delete",
    "Purge"
  ]
}

resource "azurerm_key_vault_secret" "kvs_fa_polaris_client_secret" {
  name         = "PolarisFunctionAppRegistrationClientSecret"
  value        = azuread_application_password.faap_polaris_app_service.value
  key_vault_id = azurerm_key_vault.kv_polaris.id
  depends_on = [
    azurerm_key_vault_access_policy.kvap_terraform_sp
  ]
}
