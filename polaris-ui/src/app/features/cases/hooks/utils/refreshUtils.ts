import { PipelineResults } from "../../domain/gateway/PipelineResults";

export const LOCKED_STATUS_CODE = 423;

export const isNewTime = (currentTime: string, lastTime: string) => {
  if (currentTime && !lastTime) {
    return true;
  }
  if (new Date(currentTime) > new Date(lastTime)) {
    return true;
  }
  return false;
};

export const hasDocumentUpdated = (
  document: { documentId: string; polarisDocumentVersionId: number },
  newData: PipelineResults
) => {
  const matchingDocument = newData.documents.find(
    (newDocument) => newDocument.documentId === document.documentId
  );
  if (!matchingDocument) {
    return false;
  }

  if (
    matchingDocument.polarisDocumentVersionId >
    document.polarisDocumentVersionId
  ) {
    return true;
  }
  return false;
};
