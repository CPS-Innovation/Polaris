data "azurerm_virtual_network" "polaris_vnet" {
  name                = "vnet-innovation-${var.environment.alias}"
  resource_group_name = "rg-${var.networking_resource_name_prefix}"
}

data "azurerm_subnet" "polaris_alert_notifications_subnet" {
  name                 = "${var.polaris_resource_name_prefix}-alert-notifications-subnet"
  virtual_network_name = data.azurerm_virtual_network.polaris_vnet.name
  resource_group_name  = "rg-${var.networking_resource_name_prefix}"
}

data "azurerm_key_vault" "terraform_key_vault" {
  name                = "kv${var.environment.alias}terraform"
  resource_group_name = "rg-terraform"
}

data "azurerm_application_insights" "global_ai" {
  name                = "ai-${local.global_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azurerm_log_analytics_workspace" "global_la" {
  name                = "la-${local.global_name}"
  resource_group_name = "rg-${local.analytics_group_name}"
}

data "azapi_resource_action" "callback_url_data" {
  type = "Microsoft.Web/sites/hostruntime/webhooks/api/workflows/triggers@2022-03-01"
  action = "listCallbackUrl"
  resource_id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/rg-${local.analytics_group_name}/providers/Microsoft.Web/sites/sendcaalerttotc${local.env_name_suffix}/hostruntime/runtime/webhooks/workflow/api/management/workflows/alert-processor/triggers/When_a_HTTP_request_is_received"
  response_export_values = ["*"]
}