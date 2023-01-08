resource "azurerm_eventgrid_system_topic_event_subscription" "pipeline_document_deleted_event_subscription" {
  name                   = "pipeline-storage-document-deleted-${var.env != "prod" ? var.env : ""}-event-sub"
  system_topic           = data.azurerm_eventgrid_system_topic.pipeline_document_deleted_topic.name
  resource_group_name    = "rg-${local.resource_name}"

  azure_function_endpoint {
    function_id          = "${data.azurerm_function_app.fa_text_extractor.id}/functions/HandleDocumentDeletedEvent"
  }

  included_event_types = ["Blob Deleted"]
}
