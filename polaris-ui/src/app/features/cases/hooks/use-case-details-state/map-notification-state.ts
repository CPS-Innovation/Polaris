import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { NotificationState } from "../../domain/NotificationState";

export const mapNotificationState = (
  incomingDocuments: MappedCaseDocument[],
  existingDocuments: MappedCaseDocument[],
  notificationState: NotificationState,
  dateTime: string
): NotificationState => {
  const newDocuments = incomingDocuments.filter(
    (incomingDocument) =>
      !existingDocuments.some(
        (existingDocument) =>
          existingDocument.documentId == incomingDocument.documentId
      )
  );

  const discardedDocuments = existingDocuments.filter(
    (existingDocument) =>
      !incomingDocuments.some(
        (incomingDocument) =>
          existingDocument.documentId == incomingDocument.documentId
      )
  );

  const newVersionDocuments = incomingDocuments.filter((incomingDocument) =>
    existingDocuments.some(
      (existingDocument) =>
        existingDocument.documentId == incomingDocument.documentId &&
        existingDocument.cmsVersionId != incomingDocument.cmsVersionId
    )
  );

  const reclassifiedDocuments = incomingDocuments.filter((incomingDocument) =>
    existingDocuments.some(
      (existingDocument) =>
        existingDocument.documentId == incomingDocument.documentId &&
        existingDocument.cmsVersionId == incomingDocument.cmsVersionId &&
        existingDocument.cmsDocType.documentTypeId !=
          incomingDocument.cmsDocType.documentTypeId
    )
  );

  const updatedDocuments = incomingDocuments.filter((incomingDocument) =>
    existingDocuments.some(
      (existingDocument) =>
        existingDocument.documentId == incomingDocument.documentId &&
        existingDocument.cmsVersionId == incomingDocument.cmsVersionId
    )
  );

  notificationState.events = [...notificationState.events];

  return { ...notificationState };
};
