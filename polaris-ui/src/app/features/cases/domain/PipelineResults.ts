import { PipelineDocument } from "./PipelineDocument";

export type PipelineResults = {
  transactionId: string;
  documents: PipelineDocument[];
  status:
    | "NotStarted"
    | "Running"
    // another status here for docs ready?
    | "NoDocumentsFoundInCDE"
    | "Completed"
    | "Failed";
};
