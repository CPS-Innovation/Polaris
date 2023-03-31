import { PipelineDocument } from "./PipelineDocument";

export type PipelineResults = {
  transactionId: string;
  documents: PipelineDocument[];
  status: "Running" | "DocumentsRetrieved" | "Completed" | "Failed" | "Deleted";
};
