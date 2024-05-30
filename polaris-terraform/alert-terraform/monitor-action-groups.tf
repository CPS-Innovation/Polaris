resource "azurerm_monitor_action_group" "main_polaris_action_group" {
  name                = "ag-polaris${local.env_name_suffix}-notify"
  resource_group_name = "rg-polaris-analytics${local.env_name_suffix}"
  short_name          = "polarisalert"
  email_receiver {
    email_address = "lee.spratt@cps.gov.uk"
    name          = "lee_-EmailAction-"
  }
  email_receiver {
    email_address = "rhys.williams@cps.gov.uk"
    name          = "rhys_-EmailAction-"
  }
  email_receiver {
    email_address = "joshua.king@cps.gov.uk"
    name          = "josh_-EmailAction-"
  }
  email_receiver {
    email_address = "stefan.stachow@cps.gov.uk"
    name          = "stef_-EmailAction-"
  }
  email_receiver {
    email_address = "vijay.patel@cps.gov.uk"
    name          = "vijay_-EmailAction-"
  }
  email_receiver {
    email_address = "mark.jones3@cps.gov.uk"
    name          = "mark_-EmailAction-"
  }
  email_receiver {
    email_address = "neil.foubisher@cps.gov.uk"
    name          = "neil_-EmailAction-"
  }
  logic_app_receiver {
    callback_url            = jsondecode(data.azapi_resource_action.callback_url_data.output).value
    name                    = "SendToTeamChannel"
    resource_id             = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/rg-polaris-analytics${local.env_name_suffix}/providers/Microsoft.Web/sites/SendCaAlertToTc${local.env_name_suffix}/workflows/alert-processor"
    use_common_alert_schema = true
  }
}
resource "azurerm_monitor_action_group" "res-2" {
  name                = "ag-polaris-dev-notify-nf"
  resource_group_name = "rg-polaris-analytics-dev"
  short_name          = "ag-notify-nf"
  email_receiver {
    email_address = "neil.foubister@cps.gov.uk"
    name          = "Neil.Foubister_-EmailAction-"
  }
}