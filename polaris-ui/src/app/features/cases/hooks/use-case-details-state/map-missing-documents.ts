import { CaseDocument } from "../../domain/CaseDocument";
import { PipelineResults } from "../../domain/PipelineResults";

export const mapMissingDocuments = (
  pipelineResults: PipelineResults,
  caseDocuments: CaseDocument[]
) => {
  const missingDocumentIds = pipelineResults.documents
    .filter((doc) => doc.status !== "Indexed")
    .map((doc) => doc.documentId);

  return missingDocumentIds.map((documentId) => {
    const fileName =
      caseDocuments.find(
        (caseDocument) => caseDocument.documentId === documentId
      )?.fileName || "";

    return { documentId, fileName };
  });
};
