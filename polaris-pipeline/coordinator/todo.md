Check entity id creates ok
Check for instances of NotFoundResult
Check for instances of orchestrator timeout

Is it a problem that signals are all queued but we are reading at the same time

Entity and SearchIndex stuff should not be in Common

Blob.GetDocumentAsync returning null is no good

audit routes that do/do not need to go through coordinator

Provider split up

Blobnames are wrong; pcd and dac

pdf-generator still has blobstorage reference

Need a generation on tracker -> Polaris_Metrics_CaseRefresh

Go again on authorization, make sure people cannot subvert e.g redaction or read other cases documents

Model is awful, PolarisDocumentId, the three types of document, caseId/docId are string/numeric

SaveRedactions function logic to go into a Service

OrchestratorPayload wrong

- move PolarisDocumentId up from Base

Thoughts:

- if only a subsection of docs were searchable, we would not need to add many to index, or even convert them to pdf (on demand!)
