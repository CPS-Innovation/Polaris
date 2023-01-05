import { PdfDocument } from "./PdfDocument";

export type PipelineResults = {
  transactionId: string;
  documents: PdfDocument[];
  status:
    | "NotStarted"
    | "Running"
    | "NoDocumentsFoundInCDE"
    | "Completed"
    | "Failed";
};
