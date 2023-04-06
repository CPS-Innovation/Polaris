import { PipelineDocument } from "./PipelineDocument"

export type PipelineResults = {
  transactionId: string
  documents: PipelineDocument[]
  processingCompleted?: string
  status: "Running" | "DocumentsRetrieved" | "Completed" | "Failed" | "Deleted"
}
