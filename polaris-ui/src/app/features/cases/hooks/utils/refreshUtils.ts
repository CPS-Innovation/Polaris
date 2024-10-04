import { PipelineResults } from "../../domain/gateway/PipelineResults";

export const LOCKED_STATUS_CODE = 423;

export const isNewTime = (incomingIsoTime: string, existingIsoTime: string) =>
  incomingIsoTime > existingIsoTime;

export const hasDocumentUpdated = (
  document: { documentId: string; polarisDocumentVersionId: number },
  newData: PipelineResults
) =>
  newData.documents.some(
    (newDocument) =>
      newDocument.documentId === document.documentId &&
      newDocument.polarisDocumentVersionId > document.polarisDocumentVersionId
  );
