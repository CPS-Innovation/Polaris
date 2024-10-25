import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

export const LOCKED_STATUS_CODE = 423;

export const isNewTime = (incomingIsoTime: string, existingIsoTime: string) =>
  incomingIsoTime > existingIsoTime;

export const hasDocumentUpdated = (
  document: { documentId: string; versionId: number },
  newData: PresentationDocumentProperties[]
) =>
  newData.some(
    (newDocument) =>
      newDocument.documentId === document.documentId &&
      newDocument.versionId !== document.versionId
  );
