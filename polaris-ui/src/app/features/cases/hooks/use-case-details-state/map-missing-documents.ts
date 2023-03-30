import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PipelineResults } from "../../domain/PipelineResults";

export const mapMissingDocuments = (
  pipelineResults: PipelineResults,
  caseDocuments: MappedCaseDocument[]
) => {
  const missingDocumentIds = pipelineResults.documents
    .filter((doc) => doc.status !== "Indexed")
    .map((doc) => doc.documentId);

  return missingDocumentIds.map((documentId) => {
    const fileName =
      caseDocuments.find(
        (caseDocument) => caseDocument.documentId === documentId
      )?.presentationFileName || "";

    return { documentId, fileName };
  });
};
