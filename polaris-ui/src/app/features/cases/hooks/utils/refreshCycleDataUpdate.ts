import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { RenameDocumentData } from "../../domain/gateway/RenameDocumentData";
import { ReclassifyDocumentData } from "../../domain/gateway/ReclassifyDocumentData";

export const handleRenameUpdateConfirmation = (
  documents: PresentationDocumentProperties[],
  activeRenameDoc: RenameDocumentData
) => {
  const newDocData = documents.find(
    (doc) => doc.documentId === activeRenameDoc.documentId
  );
  if (newDocData?.presentationTitle === activeRenameDoc.newName) return true;

  return false;
};

export const handleReclassifyUpdateConfirmation = (
  documents: PresentationDocumentProperties[],
  activeReclassifyDoc: ReclassifyDocumentData
) => {
  const newDocData = documents.find(
    (doc) => doc.documentId === activeReclassifyDoc.documentId
  );

  return (
    newDocData?.cmsDocType.documentTypeId === activeReclassifyDoc.newDocTypeId
  );
};
