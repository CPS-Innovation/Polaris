resource "azurerm_monitor_action_group" "standard_polaris_action_group" {
  count = var.env == "prod" ? 0 : 1
  
  name                = "ag-polaris${local.resource_suffix}-notify"
  resource_group_name = "rg-polaris-analytics${local.resource_suffix}"
  short_name          = "ag-polaris"

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
}

resource "azurerm_monitor_action_group" "multi_dest_polaris_action_group" {
  count = var.env == "prod" ? 1 : 0
  
  name                = "ag-polaris-notify"
  resource_group_name = "rg-polaris-analytics"
  short_name          = "ag-polaris"
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
    callback_url            = jsondecode(data.azapi_resource_action.callback_url_data[0].output).value
    name                    = "SendToTeamChannel"
    resource_id             = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/rg-polaris-analytics${local.resource_suffix}/providers/Microsoft.Web/sites/send-alert-teams${local.resource_suffix}/workflows/alert-processor"
    use_common_alert_schema = true
  }
}