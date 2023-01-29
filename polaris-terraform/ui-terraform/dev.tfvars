env = "dev"
location = "UK South"
environment_tag="Development"

app_service_plan_sku = {
    size = "B1"
    tier = "Basic"
}

polaris_webapp_details = {
    valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway"
	valid_scopes = "user_impersonation"
	valid_roles = ""
}