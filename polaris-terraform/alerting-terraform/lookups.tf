data "azapi_resource_action" "callback_url_data" {
  count = var.env == "prod" ? 1 : 0
  
  type = "Microsoft.Web/sites/hostruntime/webhooks/api/workflows/triggers@2022-03-01"
  action = "listCallbackUrl"
  resource_id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/rg-${local.analytics_group_name}/providers/Microsoft.Web/sites/send-alert-teams${local.resource_suffix}/hostruntime/runtime/webhooks/workflow/api/management/workflows/alert-processor/triggers/When_an_alert_is_received"
  response_export_values = ["*"]
}