#################### Functions #####################
# Ensure Minimum Tls Cipher Suite is set to TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA 
# WORKAROUND - to be done directly in the azurerm_linux_web_app resource once supported
# See the following links for more information
# https://github.com/hashicorp/terraform-provider-azurerm/issues/24223
# https://github.com/Azure/terraform-provider-azapi/issues/557
#####################################################

## Function Apps: 
locals {
  function_apps = merge(
    {
      fa_coordinator                      = azurerm_linux_function_app.fa_coordinator.id
      # fa_coordinator_staging1             = azurerm_linux_function_app_slot.fa_coordinator_staging1.id
      # fa_polaris                          = azurerm_linux_function_app.fa_polaris.id
      # fa_polaris_staging1                 = azurerm_linux_function_app_slot.fa_polaris_staging1.id
      # fa_pdf_generator                    = azurerm_windows_function_app.fa_pdf_generator.id
      # fa_pdf_generator_staging1           = azurerm_windows_function_app_slot.fa_pdf_generator_staging1.id
      # fa_pdf_redactor                     = azurerm_windows_function_app.fa_pdf_redactor.id
      # fa_pdf_redactor_staging1            = azurerm_windows_function_app_slot.fa_pdf_redactor_staging1.id
      # fa_pdf_thumbnail_generator          = azurerm_windows_function_app.fa_pdf_thumbnail_generator.id
      # fa_pdf_thumbnail_generator_staging1 = azurerm_windows_function_app_slot.fa_pdf_thumbnail_generator_staging1.id
      # fa_text_extractor                   = azurerm_linux_function_app.fa_text_extractor.id
      # fa_text_extractor_staging1          = azurerm_linux_function_app_slot.fa_text_extractor_staging1.id
    },
    var.env == "dev" ? {
      fa_polaris_maintenance              = azurerm_linux_function_app.fa_polaris_maintenance[0].id
    } : {}
  )
}

data "azapi_resource_id" "function_apps" {
  for_each = local.function_apps
  
  type      = "Microsoft.Web/sites/config@2024-11-01"
  parent_id = each.value
  name      = "web"
}


resource "azapi_resource_action" "set_min_tls_cipher_suite" {
  for_each    = local.function_apps
  type        = "Microsoft.Web/sites/config@2024-11-01"
  resource_id = data.azapi_resource_id.function_apps[each.key].id
  method      = "PUT"
  body = {
    name = data.azapi_resource_id.id[each.key].name
    properties = {
      minTlsCipherSuite = "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA"
    }
  }
  response_export_values = {
    id                = "$.id"
    name              = "$.name"
    type              = "$.type"
    minTlsCipherSuite = "$.properties.minTlsCipherSuite"
  }
}