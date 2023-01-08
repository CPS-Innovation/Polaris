provider "restapi" {
  uri                  = "https://${azurerm_search_service.ss.name}.search.windows.net"
  debug                = true
  write_returns_object = true
  alias                = "restapi_headers"
  headers = {
    Content-Type = "application/json"
    api-key = azurerm_search_service.ss.primary_key
  }
  id_attribute        = "name"
}