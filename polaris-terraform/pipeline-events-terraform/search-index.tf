provider "restapi" {
  uri                  = "https://${data.azurerm_search_service.pipeline_search_service.name}.search.windows.net"
  debug                = true
  write_returns_object = true
  alias                = "restapi_headers"
  headers = {
    Content-Type = "application/json"
    api-key      = data.azurerm_search_service.pipeline_search_service.primary_key
  }
  id_attribute = "name"
}

resource "restapi_object" "definition" {
  provider     = restapi.restapi_headers
  path         = "/indexes"
  query_string = "api-version=2021-04-30-Preview"
  data         = file("search-index-definition.json")
}