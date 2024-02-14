resource "azurerm_monitor_scheduled_query_rules_alert" "polaris-alert-rule-document-issue" {
  count = var.env == "prod" ? 1 : 0
  
  data_source_id      = data.azurerm_application_insights.global_ai.id
  frequency           = 5
  location            = data.azurerm_resource_group.rg_analytics.location
  name                = "polaris document issue reported"
  query               = "customEvents\n| where name == \"Report Document Issue\"\n| project timestamp, name, customDimensions.caseId, customDimensions.username, customDimensions.urn, customDimensions.documentId, customDimensions.correlationId\n\n"
  query_type          = "Number"
  resource_group_name = data.azurerm_resource_group.rg_analytics.name
  severity            = 2
  time_window         = 5
  action {
    action_group = [azurerm_monitor_action_group.polaris-alert-action-group[0].id]
  }
  trigger {
    operator  = "GreaterThan"
    threshold = 1
    metric_trigger {
      metric_trigger_type = "Total"
      operator            = "GreaterThanOrEqual"
      threshold           = 1
    }
  }
  depends_on = [
    azurerm_monitor_action_group.polaris-alert-action-group
  ]
}

resource "azurerm_monitor_scheduled_query_rules_alert" "polaris-alert-rule-cms504s" {
  count = var.env == "prod" ? 1 : 0
  
  data_source_id      = data.azurerm_log_analytics_workspace.global_la.id
  enabled             = false
  frequency           = 15
  location            = data.azurerm_resource_group.rg_analytics.location
  name                = "CMS-504s"
  query               = "Polaris_ProxyCmsLogs\n| where StatusCode == 504\n| project TimeGenerated, ClassicModernUiOrGraphQL, _ResourceId\n"
  query_type          = "Number"
  resource_group_name = data.azurerm_resource_group.rg_analytics.name
  severity            = 1
  time_window         = 15
  action {
    action_group = [azurerm_monitor_action_group.polaris-alert-action-group[0].id]
  }
  trigger {
    operator  = "GreaterThan"
    threshold = 1
    metric_trigger {
      metric_trigger_type = "Total"
      operator            = "GreaterThanOrEqual"
      threshold           = 1
    }
  }
  depends_on = [
    azurerm_monitor_action_group.polaris-alert-action-group
  ]
}

resource "azurerm_monitor_scheduled_query_rules_alert" "polaris-alert-rule-restricted-case-access" {
  count = var.env == "prod" ? 1 : 0
  
  data_source_id      = data.azurerm_log_analytics_workspace.global_la.id
  frequency           = 60
  location            = data.azurerm_resource_group.rg_analytics.location
  name                = "Attempt to access restricted case"
  query               = "Polaris_Bug_25015_Mirror_CMS_case_access_restrictions\n"
  query_type          = "Number"
  resource_group_name = data.azurerm_resource_group.rg_analytics.name
  severity            = 2
  time_window         = 60
  action {
    action_group = [azurerm_monitor_action_group.polaris-nf-alert-action-group[0].id]
  }
  trigger {
    operator  = "GreaterThan"
    threshold = 0
    metric_trigger {
      metric_trigger_type = "Total"
      operator            = "GreaterThanOrEqual"
      threshold           = 1
    }
  }
  depends_on = [
    azurerm_monitor_action_group.polaris-nf-alert-action-group,
  ]
}

resource "azurerm_monitor_scheduled_query_rules_alert" "polaris-alert-rule-document-converted-from-new-ext" {
  count = var.env == "prod" ? 1 : 0
  
  data_source_id      = data.azurerm_log_analytics_workspace.global_la.id
  description         = "TEST RULE: Alert when a new file extension type as part of bug 24381 has been converted to PDF"
  frequency           = 60
  location            = data.azurerm_resource_group.rg_analytics.location
  name                = "PDF converted from new file extension"
  query               = "Polaris_Bug_24381_Assorted_PDF_conversion_failures\n"
  query_type          = "Number"
  resource_group_name = data.azurerm_resource_group.rg_analytics.name
  severity            = 3
  time_window         = 60
  action {
    action_group = [azurerm_monitor_action_group.polaris-nf-alert-action-group[0].id]
  }
  trigger {
    operator  = "GreaterThan"
    threshold = 1
    metric_trigger {
      metric_trigger_type = "Total"
      operator            = "GreaterThanOrEqual"
      threshold           = 1
    }
  }
  depends_on = [
    azurerm_monitor_action_group.polaris-nf-alert-action-group
  ]
}