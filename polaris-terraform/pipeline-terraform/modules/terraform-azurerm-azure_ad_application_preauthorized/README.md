<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >=1.0.0 |
| <a name="requirement_azuread"></a> [azuread](#requirement\_azuread) | ~> 2.15.0 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | ~> 2.80 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azuread"></a> [azuread](#provider\_azuread) | ~> 2.15.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azuread_application_pre_authorized.main](https://registry.terraform.io/providers/hashicorp/azuread/latest/docs/resources/application_pre_authorized) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_application_object_id"></a> [application\_object\_id](#input\_application\_object\_id) | Object Id of the application. | `string` | n/a | yes |
| <a name="input_authorized_app_id"></a> [authorized\_app\_id](#input\_authorized\_app\_id) | Application Id of the application. | `string` | n/a | yes |
| <a name="input_permission_ids"></a> [permission\_ids](#input\_permission\_ids) | A set of permission scope IDs required by the authorized application. | `list(string)` | n/a | yes |

## Outputs

No outputs.
<!-- END_TF_DOCS -->