resource "azurerm_monitor_action_group" "polaris-alert-action-group" {
  count = var.env == "prod" ? 1 : 0
  
  name                = "ag-${local.resource_name}-notify"
  resource_group_name = data.azurerm_resource_group.rg_analytics.name
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
}

resource "azurerm_monitor_action_group" "polaris-nf-alert-action-group" {
  count = var.env == "prod" ? 1 : 0
  
  name                = "ag-${local.resource_name}-notify-nf"
  resource_group_name = data.azurerm_resource_group.rg_analytics.name
  short_name          = "ag-notify-nf"
  email_receiver {
    email_address = "neil.foubister@cps.gov.uk"
    name          = "Neil.Foubister_-EmailAction-"
  }
}