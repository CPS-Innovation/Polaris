import { PresentationDocumentProperties } from "../../domain/PdfDocument";
import { PipelineResults } from "../../domain/PipelineResults";

export const mapMissingDocuments = (
  pipelineResults: PipelineResults,
  caseDocuments: PresentationDocumentProperties[]
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
