import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { CommunicationSubCategory } from "./document-category-definitions";

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

  //slice down to reduce the unnecessary complexity when sorting
  const sliceA = a.slice(0, 12);
  const sliceB = b.slice(0, 12);
  // Get the prefix characters
  const prefixA = (sliceA.match(/^[a-zA-Z]+/g) ?? [""])[0];
  const prefixB = (sliceB.match(/^[a-zA-Z]+/g) ?? [""])[0];

  // Get the mid elements of numbers
  const midA = parseInt((sliceA.match(/\d+/g) ?? ["0"])[0]);
  const midB = parseInt((sliceB.match(/\d+/g) ?? ["0"])[0]);

  // Get the postfix characters
  const postfixA = (sliceA.match(/[a-zA-Z]$/g) ?? [""])[0];
  const postfixB = (sliceB.match(/[a-zA-Z]$/g) ?? [""])[0];

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
  if (filename.includes("UM/")) {
    return false;
  }

  //UM, must be standalone word
  if (filename.match(/(?<=^|\s)UM(?=\s|$)/gi)) {
    return true;
  }
  //UM+digit, must be standalone word,
  if (filename.match(/(?<=^|\s)UM\d+(?=\s|$)/gi)) {
    return true;
  }
  //UNUSED, standalone word,
  if (filename.match(/(?<=^|\s)UNUSED(?=\s|$)/gi)) {
    return true;
  }
  //UNUSED+digit, standalone word,
  if (filename.match(/(?<=^|\s)UNUSED\d+(?=\s|$)/gi)) {
    return true;
  }
  //-UM, as an ending/suffix to a word
  if (filename.match(/(?<=^|\s)\S+-UM(?=\s|$)/gi)) {
    return true;
  }
  //-UM+digits, as an ending/suffix to a word
  if (filename.match(/(?<=^|\s)\S+-UM\d+(?=\s|$)/gi)) {
    return true;
  }
  //MG6C, standalone word
  if (filename.match(/(?<=^|\s)MG6C(?=\s|$)/gi)) {
    return true;
  }
  //MG6D, standalone word
  if (filename.match(/(?<=^|\s)MG6D(?=\s|$)/gi)) {
    return true;
  }
  //MG6E, standalone word
  if (filename.match(/(?<=^|\s)MG6E(?=\s|$)/gi)) {
    return true;
  }
  //MG06C, standalone word
  if (filename.match(/(?<=^|\s)MG06C(?=\s|$)/gi)) {
    return true;
  }
  //MG06D, standalone word
  if (filename.match(/(?<=^|\s)MG06D(?=\s|$)/gi)) {
    return true;
  }
  //MG06E, standalone word
  if (filename.match(/(?<=^|\s)MG06E(?=\s|$)/gi)) {
    return true;
  }
  //SDC, standalone word
  if (filename.match(/(?<=^|\s)SDC(?=\s|$)/gi)) {
    return true;
  }

  return false;
};

export const getCommunicationsSubCategory = (
  doc: PresentationDocumentProperties
): CommunicationSubCategory => {
  if (doc.cmsOriginalFileExtension === ".hte")
    return CommunicationSubCategory.emails;
  return CommunicationSubCategory.communicationFiles;
};

export const getDocumentAttachments = (
  item: PresentationDocumentProperties,
  docs: PresentationDocumentProperties[]
) => {
  if (item.cmsOriginalFileExtension !== ".hte") return [];

  const attachments = docs
    .filter((doc) => doc.polarisParentDocumentId === item.documentId)
    .map(({ documentId, presentationTitle }) => {
      return {
        documentId: documentId,
        name: presentationTitle,
      };
    });
  return attachments;
};
