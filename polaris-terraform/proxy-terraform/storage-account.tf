resource "azapi_resource" "polaris_sacpspolaris_proxy_file_share" {
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01"
  name      = "polaris-proxy-content-share"
  parent_id = "${data.azurerm_subscription.current.id}/resourceGroups/${data.azurerm_resource_group.polaris_resource_group.name}/providers/Microsoft.Storage/storageAccounts/${data.azurerm_storage_account.polaris_storage_account.name}/fileServices/default"
}

resource "azurerm_storage_container" "polaris_proxy_content" {
  #checkov:skip=CKV2_AZURE_21:Ensure Storage logging is enabled for Blob service for read requests
  name                  = "content"
  storage_account_name  = data.azurerm_storage_account.polaris_storage_account.name
  container_access_type = "private"
}