resource "azurerm_storage_account" "sa_alert_processing" {
  name                            = "sacps${local.env_name}alertprocessing"
  resource_group_name             = azurerm_resource_group.rg_polaris_workspace.name
  location                        = azurerm_resource_group.rg_polaris_workspace.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  enable_https_traffic_only       = true
  public_network_access_enabled   = true
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = true

  min_tls_version = "TLS1_2"

  network_rules {
    default_action = "Deny"
    bypass         = ["Metrics", "Logging", "AzureServices"]
    virtual_network_subnet_ids = [
      azurerm_subnet.sn_polaris_alert_notifications_subnet.id
    ]
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags
}

# Create Private Endpoint for Tables
resource "azurerm_private_endpoint" "alert_processing_sa_table_pe" {
  name                = "sacps${local.env_name}alertprocessing-table-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_pipeline_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_table_storage.id]
  }

  private_service_connection {
    name                           = "sacps${local.env_name}alertprocessing-table-psc"
    private_connection_resource_id = azurerm_storage_account.sa_alert_processing.id
    is_manual_connection           = false
    subresource_names              = ["table"]
  }
}

# Create Private Endpoint for Files
resource "azurerm_private_endpoint" "alert_processing_sa_file_pe" {
  name                = "sacps${local.env_name}alertprocessing-file-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_pipeline_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_file_storage.id]
  }

  private_service_connection {
    name                           = "sacps${local.env_name}alertprocessing-file-psc"
    private_connection_resource_id = azurerm_storage_account.sa_alert_processing.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }
}

# Create Private Endpoint for Queues
resource "azurerm_private_endpoint" "alert_processing_sa_queue_pe" {
  name                = "sacps${local.env_name}alertprocessing-queue-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_pipeline_sa2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = "polaris-dns-zone-group"
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_queue_storage.id]
  }

  private_service_connection {
    name                           = "sacps${local.env_name}alertprocessing-queue-psc"
    private_connection_resource_id = azurerm_storage_account.sa_alert_processing.id
    is_manual_connection           = false
    subresource_names              = ["queue"]
  }
}

resource "azurerm_service_plan" "asp_alert_notifications" {
  name                = "asp-alert-notifications${local.env_name_suffix}"
  location            = azurerm_resource_group.rg_polaris_workspace.location
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  os_type             = "Windows"
  sku_name            = "WS1"

  tags = local.common_tags
}

resource "azurerm_logic_app_standard" "alert_notifications_processor" {
  name                       = "send-diagnostics-alert-teams${local.env_name_suffix}"
  location                   = azurerm_resource_group.rg_polaris_workspace.location
  resource_group_name        = azurerm_resource_group.rg_polaris_workspace.name
  app_service_plan_id        = azurerm_service_plan.asp_alert_notifications.id
  storage_account_name       = azurerm_storage_account.sa_alert_processing.name
  storage_account_access_key = azurerm_storage_account.sa_alert_processing.primary_access_key
  storage_account_share_name = "send-diagnostics-alert-teams${local.env_name_suffix}"
  virtual_network_subnet_id  = azurerm_subnet.sn_polaris_alert_notifications_subnet.id
  https_only                 = true
  version                    = "~4"
  
  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"        = azurerm_application_insights.ai_polaris.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.ai_polaris.connection_string
    "WEBSITE_CONTENTOVERVNET"               = "1"
    "WEBSITE_RUN_FROM_PACKAGE"              = "1"
    "WEBSITE_DNS_ALT_SERVER"                = "168.63.129.16"
    "WEBSITE_DNS_SERVER"                    = var.dns_server
  }
  
  site_config {
    use_32_bit_worker_process = false
    always_on                   = false
    dotnet_framework_version    = "v4.0"
    ftps_state                  = "Disabled"
    pre_warmed_instance_count   = "0"
    app_scale_limit             = "1"
    vnet_route_all_enabled = true
    min_tls_version = "1.2"
    public_network_access_enabled = true
    http2_enabled = true
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_private_endpoint" "alert_notifications_processor_pe" {
  name                = "${azurerm_logic_app_standard.alert_notifications_processor.name}-pe"
  resource_group_name = azurerm_resource_group.rg_polaris_workspace.name
  location            = azurerm_resource_group.rg_polaris_workspace.location
  subnet_id           = azurerm_subnet.sn_polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_logic_app_standard.alert_notifications_processor.name}-psc"
    private_connection_resource_id = azurerm_logic_app_standard.alert_notifications_processor.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}

# create teams api connection
resource "azapi_resource" "teams_api_connection" {
  type      = "Microsoft.Web/connections@2016-06-01"
  name      = "teams"
  location  = azurerm_resource_group.rg_polaris_workspace.location
  parent_id = azurerm_resource_group.rg_polaris_workspace.id
  tags      = local.common_tags

  body = <<EOF
  {
      "properties": {
          "displayName": "${var.teams_account}",
          "statuses": [
              {
                  "status": "Connected"
              }
          ],
          "parameterValues": {},
          "customParameterValues": {},
          "api": {
              "name": "teams",
              "displayName": "Microsoft Teams",
              "description": "Microsoft Teams enables you to get all your content, tools and conversations in the Team workspace with Microsoft 365.",
              "iconUri": "https://connectoricons-prod.azureedge.net/releases/v1.0.1690/1.0.1690.3719/Teams/icon.png",
              "brandColor": "#4B53BC",
              "id": "/subscriptions/${data.azurerm_client_config.current.subscription_id}/providers/Microsoft.Web/locations/${azurerm_resource_group.rg_polaris_workspace.location}/managedApis/teams",
              "type": "Microsoft.Web/locations/managedApis"
          },
          "testLinks": [
            {
              "requestUri": "'https://management.azure.com:443/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${azurerm_resource_group.rg_polaris_workspace.name}/providers/Microsoft.Web/connections/teams/extensions/proxy/beta/me/teamwork?api-version=2016-06-01",
              "method": "get"
            }
          ]
      }
  }
  EOF
  
  depends_on = [azurerm_logic_app_standard.alert_notifications_processor]
}

resource "azapi_resource" "logic_app" {
  type      = "Microsoft.Logic/workflows@2016-06-01"
  location  = azurerm_resource_group.rg_polaris_workspace.location
  parent_id = azurerm_resource_group.rg_polaris_workspace.id
  name      = "alert-processor${local.env_name_suffix}"
  tags      = local.common_tags
  identity {
    type = "SystemAssigned"
  }
  body = jsonencode({
    "properties" : {
      "state" : "Enabled",
      "definition": {
        "$schema": "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#",
        "actions": {
          "Switch": {
            "type": "Switch",
            "expression": "@triggerBody()?['data']?['essentials']?['monitoringService']",
            "default": {
              "actions": {
                "Send_General_Alert": {
                  "type": "ApiConnection",
                  "inputs": {
                    "host": {
                      "connection": {
                        "referenceName": "teams"
                      }
                    },
                    "method": "post",
                    "body": {
                      "recipient": {
                        "groupId": "611db2ac-4627-44ad-923b-abb04ef0e385",
                        "channelId": "19:a67baafbdfea4e1cbb6fea96f449be1a@thread.tacv2"
                      },
                      "messageBody": "<p><b><strong>Azure Monitor Alert</strong></b></p><br><p><span style=\"\\&quot;font-size:\">Basic Information</span><span style=\"\\&quot;font-size:\">:</span></p><p><span style=\"\\&quot;font-size:\">- Alert Id: </span>@{triggerBody()?['data']?['essentials']?['alertId']}</p><p>- Alert Rule Fired: @{triggerBody()?['data']?['essentials']?['alertRule']}</p><p>- Serverity: @{triggerBody()?['data']?['essentials']?['severity']}</p><p>- Description: @{triggerBody()?['data']?['essentials']?['description']}</p><p>- Fired at: @{triggerBody()?['data']?['essentials']?['firedDateTime']}</p><p>- Resolved at: @{triggerBody()?['data']?['essentials']?['resolvedDateTime']}</p><br><p>Additional Details:</p><p>- Monitor Condition: @{triggerBody()?['data']?['essentials']?['monitorCondition']}</p><p>- Monitoring Service: @{triggerBody()?['data']?['essentials']?['monitoringService']}</p><br><p>Metric Information:</p><p>- Namespace: @{item()?['metricNamespace']}</p><p>- Metric Name: @{item()?['metricName']}</p><p>- Threshold: @{item()?['threshold']}</p><p>- Metric Value: @{item()?['metricValue']}</p>"
                    },
                    "path": "/beta/teams/conversation/message/poster/@{encodeURIComponent('Flow bot')}/location/@{encodeURIComponent('Channel')}"
                  }
                }
              }
            },
            "cases": {
              "Case": {
                "actions": {
                  "Post_message_in_a_chat_or_channel": {
                    "type": "ApiConnection",
                    "inputs": {
                      "host": {
                        "connection": {
                          "referenceName": "teams"
                        }
                      },
                      "method": "post",
                      "body": {
                        "recipient": {
                          "groupId": "611db2ac-4627-44ad-923b-abb04ef0e385",
                          "channelId": "19:a67baafbdfea4e1cbb6fea96f449be1a@thread.tacv2"
                        },
                        "messageBody": "<p><b><strong>Azure Monitor Alert</strong></b></p><br><p><span style=\"\\&quot;font-size:\">Basic Information</span><span style=\"\\&quot;font-size:\">:</span></p><p><span style=\"\\&quot;font-size:\">- Alert Id: </span>@{triggerBody()?['data']?['essentials']?['alertId']}</p><p>- Alert Rule Fired: @{triggerBody()?['data']?['essentials']?['alertRule']}</p><p>- Serverity: @{triggerBody()?['data']?['essentials']?['severity']}</p><p>- Description: @{triggerBody()?['data']?['essentials']?['description']}</p><p>- Fired at: @{triggerBody()?['data']?['essentials']?['firedDateTime']}</p><p>- Resolved at: @{triggerBody()?['data']?['essentials']?['resolvedDateTime']}</p><br><p>Additional Details:</p><p>- Monitor Condition: @{triggerBody()?['data']?['essentials']?['monitorCondition']}</p><p>- Monitoring Service: @{triggerBody()?['data']?['essentials']?['monitoringService']}</p><br><p>Metric Information:</p><p>- Namespace: @{item()?['metricNamespace']}</p><p>- Metric Name: @{item()?['metricName']}</p><p>- Threshold: @{item()?['threshold']}</p><p>- Metric Value: @{item()?['metricValue']}</p>"
                      },
                      "path": "/beta/teams/conversation/message/poster/Flow bot/location/@{encodeURIComponent('Channel')}"
                    }
                  }
                },
                "case": "Platform"
              },
              "Case 2": {
                "actions": {
                  "Send_Log_Analytics_Alert": {
                    "type": "ApiConnection",
                    "inputs": {
                      "host": {
                        "connection": {
                          "referenceName": "teams"
                        }
                      },
                      "method": "post",
                      "body": {
                        "recipient": {
                          "groupId": "611db2ac-4627-44ad-923b-abb04ef0e385",
                          "channelId": "19:a67baafbdfea4e1cbb6fea96f449be1a@thread.tacv2"
                        },
                        "messageBody": "<p><b><strong>Azure Monitor Alert</strong></b></p><br><p><span style=\"\\&quot;font-size:\">Basic Information</span><span style=\"\\&quot;font-size:\">:</span></p><p><span style=\"\\&quot;font-size:\">- Alert Id: </span>@{triggerBody()?['data']?['essentials']?['alertId']}</p><p>- Alert Rule Fired: @{triggerBody()?['data']?['essentials']?['alertRule']}</p><p>- Serverity: @{triggerBody()?['data']?['essentials']?['severity']}</p><p>- Description: @{triggerBody()?['data']?['essentials']?['description']}</p><p>- Fired at: @{triggerBody()?['data']?['essentials']?['firedDateTime']}</p><p>- Resolved at: @{triggerBody()?['data']?['essentials']?['resolvedDateTime']}</p><br><p>Additional Details:</p><p>- Monitor Condition: @{triggerBody()?['data']?['essentials']?['monitorCondition']}</p><p>- Monitoring Service: @{triggerBody()?['data']?['essentials']?['monitoringService']}</p><br><p>Metric Information:</p><p>- Namespace: @{item()?['metricNamespace']}</p><p>- Metric Name: @{item()?['metricName']}</p><p>- Threshold: @{item()?['threshold']}</p><p>- Metric Value: @{item()?['metricValue']}</p>"
                      },
                      "path": "/beta/teams/conversation/message/poster/@{encodeURIComponent('Flow bot')}/location/@{encodeURIComponent('Channel')}"
                    }
                  }
                },
                "case": "Log Analytics"
              },
              "Case 3": {
                "actions": {
                  "Send_App_Insights_Alert": {
                    "type": "ApiConnection",
                    "inputs": {
                      "host": {
                        "connection": {
                          "referenceName": "teams"
                        }
                      },
                      "method": "post",
                      "body": {
                        "recipient": {
                          "groupId": "611db2ac-4627-44ad-923b-abb04ef0e385",
                          "channelId": "19:a67baafbdfea4e1cbb6fea96f449be1a@thread.tacv2"
                        },
                        "messageBody": "<p><b><strong>Azure Monitor Alert</strong></b></p><br><p><span style=\"\\&quot;font-size:\">Basic Information</span><span style=\"\\&quot;font-size:\">:</span></p><p><span style=\"\\&quot;font-size:\">- Alert Id: </span>@{triggerBody()?['data']?['essentials']?['alertId']}</p><p>- Alert Rule Fired: @{triggerBody()?['data']?['essentials']?['alertRule']}</p><p>- Serverity: @{triggerBody()?['data']?['essentials']?['severity']}</p><p>- Description: @{triggerBody()?['data']?['essentials']?['description']}</p><p>- Fired at: @{triggerBody()?['data']?['essentials']?['firedDateTime']}</p><p>- Resolved at: @{triggerBody()?['data']?['essentials']?['resolvedDateTime']}</p><br><p>Additional Details:</p><p>- Monitor Condition: @{triggerBody()?['data']?['essentials']?['monitorCondition']}</p><p>- Monitoring Service: @{triggerBody()?['data']?['essentials']?['monitoringService']}</p><br><p>Metric Information:</p><p>- Namespace: @{item()?['metricNamespace']}</p><p>- Metric Name: @{item()?['metricName']}</p><p>- Threshold: @{item()?['threshold']}</p><p>- Metric Value: @{item()?['metricValue']}</p>"
                      },
                      "path": "/beta/teams/conversation/message/poster/@{encodeURIComponent('Flow bot')}/location/@{encodeURIComponent('Channel')}"
                    }
                  }
                },
                "case": "Application Insights"
              },
              "Case 4": {
                "actions": {
                  "Send_Log_Alert": {
                    "type": "ApiConnection",
                    "inputs": {
                      "host": {
                        "connection": {
                          "referenceName": "teams"
                        }
                      },
                      "method": "post",
                      "body": {
                        "recipient": {
                          "groupId": "611db2ac-4627-44ad-923b-abb04ef0e385",
                          "channelId": "19:a67baafbdfea4e1cbb6fea96f449be1a@thread.tacv2"
                        },
                        "messageBody": "<p><b><strong>Azure Monitor Alert</strong></b></p><br><p><span style=\"\\&quot;font-size:\">Basic Information</span><span style=\"\\&quot;font-size:\">:</span></p><p><span style=\"\\&quot;font-size:\">- Alert Id: </span>@{triggerBody()?['data']?['essentials']?['alertId']}</p><p>- Alert Rule Fired: @{triggerBody()?['data']?['essentials']?['alertRule']}</p><p>- Serverity: @{triggerBody()?['data']?['essentials']?['severity']}</p><p>- Description: @{triggerBody()?['data']?['essentials']?['description']}</p><p>- Fired at: @{triggerBody()?['data']?['essentials']?['firedDateTime']}</p><p>- Resolved at: @{triggerBody()?['data']?['essentials']?['resolvedDateTime']}</p><br><p>Additional Details:</p><p>- Monitor Condition: @{triggerBody()?['data']?['essentials']?['monitorCondition']}</p><p>- Monitoring Service: @{triggerBody()?['data']?['essentials']?['monitoringService']}</p><br><p>Metric Information:</p><p>- Namespace: @{item()?['metricNamespace']}</p><p>- Metric Name: @{item()?['metricName']}</p><p>- Threshold: @{item()?['threshold']}</p><p>- Metric Value: @{item()?['metricValue']}</p>"
                      },
                      "path": "/beta/teams/conversation/message/poster/@{encodeURIComponent('Flow bot')}/location/@{encodeURIComponent('Channel')}"
                    }
                  }
                },
                "case": "Log Alerts V2"
              },
              "Case 5": {
                "actions": {
                  "Send_Service_Health_Alert": {
                    "type": "ApiConnection",
                    "inputs": {
                      "host": {
                        "connection": {
                          "referenceName": "teams"
                        }
                      },
                      "method": "post",
                      "body": {
                        "recipient": {
                          "groupId": "611db2ac-4627-44ad-923b-abb04ef0e385",
                          "channelId": "19:a67baafbdfea4e1cbb6fea96f449be1a@thread.tacv2"
                        },
                        "messageBody": "<p><b><strong>Azure Monitor Alert</strong></b></p><br><p><span style=\"\\&quot;font-size:\">Basic Information</span><span style=\"\\&quot;font-size:\">:</span></p><p><span style=\"\\&quot;font-size:\">- Alert Id: </span>@{triggerBody()?['data']?['essentials']?['alertId']}</p><p>- Alert Rule Fired: @{triggerBody()?['data']?['essentials']?['alertRule']}</p><p>- Serverity: @{triggerBody()?['data']?['essentials']?['severity']}</p><p>- Description: @{triggerBody()?['data']?['essentials']?['description']}</p><p>- Fired at: @{triggerBody()?['data']?['essentials']?['firedDateTime']}</p><p>- Resolved at: @{triggerBody()?['data']?['essentials']?['resolvedDateTime']}</p><br><p>Additional Details:</p><p>- Monitor Condition: @{triggerBody()?['data']?['essentials']?['monitorCondition']}</p><p>- Monitoring Service: @{triggerBody()?['data']?['essentials']?['monitoringService']}</p><br><p>Metric Information:</p><p>- Namespace: @{item()?['metricNamespace']}</p><p>- Metric Name: @{item()?['metricName']}</p><p>- Threshold: @{item()?['threshold']}</p><p>- Metric Value: @{item()?['metricValue']}</p>"
                      },
                      "path": "/beta/teams/conversation/message/poster/@{encodeURIComponent('Flow bot')}/location/@{encodeURIComponent('Channel')}"
                    }
                  }
                },
                "case": "Service Health"
              },
              "Case 6": {
                "actions": {
                  "Send_Resource_Health_Alert": {
                    "type": "ApiConnection",
                    "inputs": {
                      "host": {
                        "connection": {
                          "referenceName": "teams"
                        }
                      },
                      "method": "post",
                      "body": {
                        "recipient": {
                          "groupId": "611db2ac-4627-44ad-923b-abb04ef0e385",
                          "channelId": "19:a67baafbdfea4e1cbb6fea96f449be1a@thread.tacv2"
                        },
                        "messageBody": "<p><b><strong>Azure Monitor Alert</strong></b></p><br><p><span style=\"\\&quot;font-size:\">Basic Information</span><span style=\"\\&quot;font-size:\">:</span></p><p><span style=\"\\&quot;font-size:\">- Alert Id: </span>@{triggerBody()?['data']?['essentials']?['alertId']}</p><p>- Alert Rule Fired: @{triggerBody()?['data']?['essentials']?['alertRule']}</p><p>- Serverity: @{triggerBody()?['data']?['essentials']?['severity']}</p><p>- Description: @{triggerBody()?['data']?['essentials']?['description']}</p><p>- Fired at: @{triggerBody()?['data']?['essentials']?['firedDateTime']}</p><p>- Resolved at: @{triggerBody()?['data']?['essentials']?['resolvedDateTime']}</p><br><p>Additional Details:</p><p>- Monitor Condition: @{triggerBody()?['data']?['essentials']?['monitorCondition']}</p><p>- Monitoring Service: @{triggerBody()?['data']?['essentials']?['monitoringService']}</p><br><p>Metric Information:</p><p>- Namespace: @{item()?['metricNamespace']}</p><p>- Metric Name: @{item()?['metricName']}</p><p>- Threshold: @{item()?['threshold']}</p><p>- Metric Value: @{item()?['metricValue']}</p>"
                      },
                      "path": "/beta/teams/conversation/message/poster/@{encodeURIComponent('Flow bot')}/location/@{encodeURIComponent('Channel')}"
                    }
                  }
                },
                "case": "Resource Health"
              }
            },
            "runAfter": {}
          }
        },
        "contentVersion": "1.0.0.0",
        "outputs": {},
        "triggers": {
          "When_an_alert_is_received": {
            "type": "Request",
            "kind": "Http",
            "inputs": {
              "schema": {
                "properties": {
                  "data": {
                    "properties": {
                      "alertContext": {
                        "properties": {
                          "condition": {
                            "properties": {
                              "allOf": {
                                "items": {
                                  "properties": {
                                    "dimensions": {
                                      "items": {
                                        "properties": {
                                          "name": {
                                            "type": "string"
                                          },
                                          "value": {
                                            "type": "string"
                                          }
                                        },
                                        "required": [
                                          "name",
                                          "value"
                                        ],
                                        "type": "object"
                                      },
                                      "type": "array"
                                    },
                                    "metricName": {
                                      "type": "string"
                                    },
                                    "metricNamespace": {
                                      "type": "string"
                                    },
                                    "metricValue": {
                                      "type": "number"
                                    },
                                    "operator": {
                                      "type": "string"
                                    },
                                    "threshold": {
                                      "type": "string"
                                    },
                                    "timeAggregation": {
                                      "type": "string"
                                    }
                                  },
                                  "required": [
                                    "metricName",
                                    "metricNamespace",
                                    "operator",
                                    "threshold",
                                    "timeAggregation",
                                    "dimensions",
                                    "metricValue"
                                  ],
                                  "type": "object"
                                },
                                "type": "array"
                              },
                              "windowSize": {
                                "type": "string"
                              }
                            },
                            "type": "object"
                          },
                          "conditionType": {
                            "type": "string"
                          },
                          "properties": {}
                        },
                        "type": "object"
                      },
                      "customProperties": {
                        "properties": {
                          "Key1": {
                            "type": "string"
                          },
                          "Key2": {
                            "type": "string"
                          }
                        },
                        "type": "object"
                      },
                      "essentials": {
                        "properties": {
                          "alertContextVersion": {
                            "type": "string"
                          },
                          "alertId": {
                            "type": "string"
                          },
                          "alertRule": {
                            "type": "string"
                          },
                          "alertTargetIDs": {
                            "items": {
                              "type": "string"
                            },
                            "type": "array"
                          },
                          "configurationItems": {
                            "items": {
                              "type": "string"
                            },
                            "type": "array"
                          },
                          "description": {
                            "type": "string"
                          },
                          "essentialsVersion": {
                            "type": "string"
                          },
                          "firedDateTime": {
                            "type": "string"
                          },
                          "monitorCondition": {
                            "type": "string"
                          },
                          "monitoringService": {
                            "type": "string"
                          },
                          "originAlertId": {
                            "type": "string"
                          },
                          "resolvedDateTime": {
                            "type": "string"
                          },
                          "severity": {
                            "type": "string"
                          },
                          "signalType": {
                            "type": "string"
                          }
                        },
                        "type": "object"
                      }
                    },
                    "type": "object"
                  },
                  "schemaId": {
                    "type": "string"
                  }
                },
                "type": "object"
              }
            },
            "operationOptions": "EnableSchemaValidation"
          }
        }
      },
      "connectionReferences": {
        "teams": {
          "connection": {
            "id": "/subscriptions/888d158f-ade3-4528-9c59-84bd2260d5ee/resourceGroups/rg-polaris-analytics/providers/Microsoft.Web/connections/teams"
          },
          "connectionName": "teams",
          "api": {
            "id": "/subscriptions/888d158f-ade3-4528-9c59-84bd2260d5ee/providers/Microsoft.Web/locations/uksouth/managedApis/teams"
          },
          "authentication": {
            "type": "ManagedServiceIdentity"
          }
        }
      },
      "parameters": {}
    }
  })
}
