#################### Key Vault ####################

resource "azurerm_key_vault" "kv_polaris" {
  name                = "kv-${local.resource_name}"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  enabled_for_deployment          = true
  enabled_for_template_deployment = true
  enable_rbac_authorization       = true
  purge_protection_enabled        = true
  soft_delete_retention_days      = 90

  sku_name = "standard"

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    virtual_network_subnet_ids = [
      data.azurerm_subnet.polaris_ci_subnet.id,
      data.azurerm_subnet.polaris_gateway_subnet.id
    ]
  }
  tags = local.common_tags
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_key_vault_pe" {
  name                = "${azurerm_key_vault.kv_polaris.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_keyvault.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_keyvault.id]
  }

  private_service_connection {
    name                           = "${azurerm_key_vault.kv_polaris.name}-psc"
    private_connection_resource_id = azurerm_key_vault.kv_polaris.id
    is_manual_connection           = false
    subresource_names              = ["vault"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_key_vault_dns_a" {
  name                = azurerm_key_vault.kv_polaris.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_keyvault.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_key_vault_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}

resource "azurerm_key_vault_secret" "kvs_fa_polaris_client_secret" {
  name         = "PolarisFunctionAppRegistrationClientSecret"
  value        = azuread_application_password.faap_polaris_app_service.value
  key_vault_id = azurerm_key_vault.kv_polaris.id
  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azurerm_key_vault_access_policy.kvap_terraform_sp
  ]
}

resource "azurerm_role_assignment" "kv_role_terraform_sp" {
  scope                = azurerm_key_vault.kv_polaris.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
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

resource "azurerm_role_assignment" "kv_role_fa_gateway_crypto_user" {
  scope                = azurerm_key_vault.kv_polaris.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_linux_function_app.fa_polaris.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_gateway_secrets_user" {
  scope                = azurerm_key_vault.kv_polaris.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_function_app.fa_polaris.identity[0].principal_id
}

resource "azurerm_key_vault_secret" "kvs_ui_storage_connection_string" {
  name            = "UiStorageConnectionString"
  value           = azurerm_storage_account.sacpspolaris.primary_connection_string
  key_vault_id    = azurerm_key_vault.kv_polaris.id
  expiration_date = timeadd(timestamp(), "8760h")
  content_type    = "password"

  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azurerm_storage_account.sacpspolaris
  ]
}