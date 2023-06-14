import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

export const sortDocumentsByCreatedDate = (
  cmsDocuments: PresentationDocumentProperties[],
  sortOrder: "ascending" | "descending" = "ascending"
) => {
  const compareFunction =
    sortOrder === "ascending"
      ? (
          a: PresentationDocumentProperties,
          b: PresentationDocumentProperties
        ) =>
          new Date(a.cmsFileCreatedDate).getTime() -
          new Date(b.cmsFileCreatedDate).getTime()
      : (
          a: PresentationDocumentProperties,
          b: PresentationDocumentProperties
        ) =>
          new Date(b.cmsFileCreatedDate).getTime() -
          new Date(a.cmsFileCreatedDate).getTime();

  return cmsDocuments.sort(compareFunction);
};

export const sortAscendingByDocumentTypeAndCreationDate = (
  cmsDocuments: PresentationDocumentProperties[]
) => {
  const compareFunction = (
    a: PresentationDocumentProperties,
    b: PresentationDocumentProperties
  ) => {
    if (a.cmsDocType.documentType !== b.cmsDocType.documentType) {
      return a.cmsDocType.documentType < b.cmsDocType.documentType ? -1 : 1;
    } else {
      return (
        new Date(a.cmsFileCreatedDate).getTime() -
        new Date(b.cmsFileCreatedDate).getTime()
      );
    }
  };

  return cmsDocuments.sort(compareFunction);
};

export const sortAscendingByListOrderAndId = (
  cmsDocuments: PresentationDocumentProperties[]
) => {
  const compareFunction = (
    a: PresentationDocumentProperties,
    b: PresentationDocumentProperties
  ) => {
    // if (a.documentId !== b.documentId) {
    //   return a.documentId.localeCompare(b.documentId);
    // } else {
    return a.documentId < b.documentId ? -1 : 1;
  };
  return cmsDocuments.sort(compareFunction);
};
