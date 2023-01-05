resource "azuread_service_principal" "main" {
  # Mandatory arguments
  application_id = var.application_id

  #Option arguments
  account_enabled               = var.account_enabled
  alternative_names             = var.alternative_names
  app_role_assignment_required  = var.app_role_assignment_required
  description                   = var.description
  use_existing                  = var.use_existing
  login_url                     = var.login_url
  notes                         = var.notes
  notification_email_addresses  = var.notification_email_addresses
  owners                        = var.owners
  preferred_single_sign_on_mode = var.preferred_single_sign_on_mode

  dynamic "saml_single_sign_on" {
    for_each = var.saml_single_sign_on != null ? ["true"] : []
    content {
      relay_state = lookup(var.saml_single_sign_on, "relay_state", null)
    }
  }

  dynamic "feature_tags" {
    for_each = var.feature_tags != null ? ["true"] : []
    content {
      custom_single_sign_on = lookup(var.feature_tags, "custom_single_sign_on", null)
      enterprise            = lookup(var.feature_tags, "enterprise", null)
      gallery               = lookup(var.feature_tags, "gallery", null)
      hide                  = lookup(var.feature_tags, "hide", null)
    }
  }
}



