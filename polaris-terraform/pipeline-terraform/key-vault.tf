#################### Key Vault #####################

resource "azurerm_key_vault" "kv" {
  name                = "kv-${local.resource_name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  enabled_for_deployment          = true
  enabled_for_template_deployment = true
  enabled_for_disk_encryption     = true
  enable_rbac_authorization       = true
  purge_protection_enabled        = true
  soft_delete_retention_days      = 90
  public_network_access_enabled   = false

  sku_name = "standard"

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    virtual_network_subnet_ids = [
      data.azurerm_subnet.polaris_ci_subnet.id,
      data.azurerm_subnet.polaris_coordinator_subnet.id,
      data.azurerm_subnet.polaris_pdfgenerator_subnet.id,
      data.azurerm_subnet.polaris_textextractor_subnet.id
    ]
  }

  tags = local.common_tags
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_key_vault_pe" {
  name                = "${azurerm_key_vault.kv.name}-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_key_vault_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_keyvault.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_keyvault.id]
  }

  private_service_connection {
    name                           = "${azurerm_key_vault.kv.name}-psc"
    private_connection_resource_id = azurerm_key_vault.kv.id
    is_manual_connection           = false
    subresource_names              = ["vault"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "pipeline_key_vault_dns_a" {
  name                = azurerm_key_vault.kv.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_keyvault.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.pipeline_key_vault_pe.private_service_connection.0.private_ip_address]
  tags                = local.common_tags
}

resource "azurerm_key_vault_secret" "kvs_fa_coordinator_client_secret" {
  name            = "CoordinatorFunctionAppRegistrationClientSecret"
  value           = azuread_application_password.faap_fa_coordinator_app_service.value
  key_vault_id    = azurerm_key_vault.kv.id
  expiration_date = timeadd(timestamp(), "8760h")
  content_type    = "password"
  tags            = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azurerm_key_vault_access_policy.kvap_terraform_sp
  ]
}

resource "azurerm_key_vault_secret" "kvs_fa_pdf_generator_client_secret" {
  name            = "PdfGeneratorFunctionAppRegistrationClientSecret"
  value           = azuread_application_password.faap_fa_pdf_generator_app_service.value
  key_vault_id    = azurerm_key_vault.kv.id
  expiration_date = timeadd(timestamp(), "8760h")
  content_type    = "password"
  tags            = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azurerm_key_vault_access_policy.kvap_terraform_sp
  ]
}

resource "azurerm_role_assignment" "kv_role_terraform_sp" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
}

resource "azurerm_role_assignment" "terraform_kv_role_terraform_sp" {
  scope                = data.azurerm_key_vault.terraform_key_vault.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
}

resource "azurerm_key_vault_access_policy" "kvap_terraform_sp" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azuread_service_principal.terraform_service_principal.object_id

  secret_permissions = [
    "Get",
    "Set",
    "Delete",
    "Purge"
  ]
}

resource "azurerm_key_vault_access_policy" "terraform_kvap_terraform_sp" {
  key_vault_id = data.azurerm_key_vault.terraform_key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azuread_service_principal.terraform_service_principal.object_id

  secret_permissions = [
    "Get",
    "Set",
    "Delete",
    "Purge"
  ]
}

resource "azurerm_role_assignment" "kv_role_fa_coordinator_crypto_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_linux_function_app.fa_coordinator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_coordinator_secrets_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_function_app.fa_coordinator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_pdf_generator_crypto_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_windows_function_app.fa_pdf_generator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_pdf_generator_secrets_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_windows_function_app.fa_pdf_generator.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_text_extractor_crypto_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_linux_function_app.fa_text_extractor.identity[0].principal_id
}

resource "azurerm_role_assignment" "kv_role_fa_text_extractor_secrets_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_function_app.fa_text_extractor.identity[0].principal_id
}

resource "azurerm_key_vault_secret" "kvs_pipeline_terraform_storage_connection_string" {
  name            = "cpsdocumentstorage-connection-string"
  value           = azurerm_storage_account.sa.primary_connection_string
  key_vault_id    = data.azurerm_key_vault.terraform_key_vault.id
  expiration_date = timeadd(timestamp(), "8760h")
  content_type    = "password"

  depends_on = [
    azurerm_role_assignment.terraform_kv_role_terraform_sp,
    azurerm_key_vault_access_policy.terraform_kvap_terraform_sp,
    azurerm_storage_account.sa
  ]
}
