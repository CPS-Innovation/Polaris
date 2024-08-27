import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { RenameDocumentData } from "../../domain/gateway/RenameDocumentData";

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
