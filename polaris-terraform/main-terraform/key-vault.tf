#################### Key Vault - UI ####################

resource "azurerm_key_vault" "kv_polaris" {
  name                = "kv-${local.global_resource_name}"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  enabled_for_deployment          = true
  enabled_for_template_deployment = true
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

resource "azurerm_key_vault_secret" "kvs_fa_polaris_client_secret" {
  #checkov:skip=CKV_AZURE_41:Ensure that the expiration date is set on all secrets
  #checkov:skip=CKV_AZURE_114:Ensure that key vault secrets have "content_type" set
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

#################### Key Vault - Pipeline #####################

resource "azurerm_key_vault" "kv" {
  name                = "kv-${local.pipeline_resource_name}"
  location            = azurerm_resource_group.rg_polaris_pipeline.location
  resource_group_name = azurerm_resource_group.rg_polaris_pipeline.name
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
      data.azurerm_subnet.polaris_textextractor_2_subnet.id
    ]
  }

  tags = local.common_tags
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_key_vault_pe" {
  name                = "${azurerm_key_vault.kv.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_pipeline.name
  location            = azurerm_resource_group.rg_polaris_pipeline.location
  subnet_id           = data.azurerm_subnet.polaris_sa_subnet.id
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

resource "azurerm_role_assignment" "kvs_role_terraform_sp" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
}

resource "azurerm_role_assignment" "terraform_kv_role_terraform_sp" {
  scope                = data.azurerm_key_vault.terraform_key_vault.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
}

resource "azurerm_key_vault_access_policy" "kvsap_terraform_sp" {
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
