env = "qa"
location = "UK South"
environment_tag="QA"
stub_blob_storage_connection_string="DefaultEndpointsProtocol=https;AccountName=saqacmsdocumentservices;AccountKey=nmbdArGrAOzr2nHk1srJkzt2lURPPnFEW5pUfx/oGFlT08Ec70RC6uzdNDXOJjM/rKq5X3g/1A70Zk92HR044Q==;EndpointSuffix=core.windows.net"

app_service_plan_sku = {
    size = "B1"
    tier = "Basic"
}

rumpole_webapp_details = {
    valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-rumpole-qa-gateway"
    valid_scopes = "user_impersonation"
    valid_roles = ""
}

core_data_api_details = {
    api_id = "cf7e64d9-928d-4cab-a957-3bb64f5ea733"
    api_url = "https://core-data.dev.cpsdigital.co.uk/graphql"
    api_scope = "api://5f1f433a-41b3-45d3-895e-927f50232a47/case.confirm"
    case_confirm_user_impersonation_id = "0b26cc45-00d5-403f-9534-0044267c41de"
}