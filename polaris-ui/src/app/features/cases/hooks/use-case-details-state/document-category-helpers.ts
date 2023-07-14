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
      return customSortByDocumentType(
        a.cmsDocType.documentType,
        b.cmsDocType.documentType
      );
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
    if (
      a.categoryListOrder !== b.categoryListOrder &&
      a.categoryListOrder &&
      b.categoryListOrder
    ) {
      return a.categoryListOrder < b.categoryListOrder ? -1 : 1;
    } else {
      return a.documentId < b.documentId ? -1 : 1;
    }
  };
  return cmsDocuments.sort(compareFunction);
};

export const customSortByDocumentType = (a: string, b: string): number => {
  if (!a) return 1;
  if (!b) return -1;
  // Get the prefix characters
  const prefixA = (a.match(/^[a-zA-Z]+/) || [""])[0];
  const prefixB = (b.match(/^[a-zA-Z]+/) || [""])[0];

  // Get the mid elements of numbers
  const midA = parseInt((a.match(/\d+/) || ["0"])[0]);
  const midB = parseInt((b.match(/\d+/) || ["0"])[0]);

  // Get the postfix characters
  const postfixA = (a.match(/[a-zA-Z]+$/u) || [""])[0];
  const postfixB = (b.match(/[a-zA-Z]+$/u) || [""])[0];

  // Sort based on categories
  if (prefixA !== prefixB) {
    return prefixA < prefixB ? -1 : 1;
  } else if (midA !== midB) {
    return midA - midB;
  } else {
    return postfixA < postfixB ? -1 : 1;
  }
};

export const isUnusedCommunicationMaterial = (
  filename: string,
  documentTypeId: number
) => {
  if (!filename) {
    return false;
  }

  if (documentTypeId !== 1029) {
    return false;
  }
  if (filename.includes("UM")) {
    return true;
  }
  // Check if the filename contains "Item N" (where N is an integer)
  const regex = /Item\s\d+/;
  const matches = filename.match(regex);

  if (matches) {
    return true;
  }

  return false;
};
