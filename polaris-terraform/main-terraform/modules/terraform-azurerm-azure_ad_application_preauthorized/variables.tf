variable "application_object_id" {
  type        = string
  description = "Object Id of the application."
}

variable "authorized_app_id" {
  type        = string
  description = "Application Id of the application."
}

variable "permission_ids" {
  type        = list(string)
  description = "A set of permission scope IDs required by the authorized application."
}
