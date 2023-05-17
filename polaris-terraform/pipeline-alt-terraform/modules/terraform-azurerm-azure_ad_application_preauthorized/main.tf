
resource "azuread_application_pre_authorized" "main" {
  application_object_id = var.application_object_id
  authorized_app_id     = var.authorized_app_id
  permission_ids        = var.permission_ids
}