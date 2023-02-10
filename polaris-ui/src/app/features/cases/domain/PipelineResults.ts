import { PdfDocument } from "./PdfDocument";

export type PipelineResults = {
  transactionId: string;
  documents: PdfDocument[];
  status:
    | "NotStarted"
    | "Running"
    // another status here for docs ready?
    | "NoDocumentsFoundInCDE"
    | "Completed"
    | "Failed";
};
