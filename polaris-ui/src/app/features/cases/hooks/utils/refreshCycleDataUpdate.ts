import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { RenameDocumentData } from "../../domain/gateway/RenameDocumentData";
import { ReclassifyDocumentData } from "../../domain/gateway/ReclassifyDocumentData";

export const handleRenameUpdateConfirmation = (
  pipelineResults: PipelineResults,
  activeRenameDoc: RenameDocumentData
) => {
  const newDocData = pipelineResults.documents.find(
    (doc) => doc.documentId === activeRenameDoc.documentId
  );
  if (newDocData?.presentationTitle === activeRenameDoc.newName) return true;

  return false;
};

export const handleReclassifyUpdateConfirmation = (
  pipelineResults: PipelineResults,
  activeReclassifyDoc: ReclassifyDocumentData
) => {
  const newDocData = pipelineResults.documents.find(
    (doc) => doc.documentId === activeReclassifyDoc.documentId
  );
  if (
    newDocData?.cmsDocType.documentTypeId === activeReclassifyDoc.newDocTypeId
  )
    return true;

  return true;
};
