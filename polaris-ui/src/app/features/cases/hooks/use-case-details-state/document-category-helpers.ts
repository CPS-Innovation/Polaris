import { CmsDocType } from "../../domain/gateway/CmsDocType";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { CommunicationSubCategory } from "./document-category-definitions";

export const sortDocumentsByCreatedDate = <
  T extends PresentationDocumentProperties
>(
  cmsDocuments: T[],
  sortOrder: "ascending" | "descending" = "ascending"
) => {
  const compareFunction = (a: T, b: T) =>
    sortOrder === "ascending"
      ? new Date(a.cmsFileCreatedDate).getTime() -
        new Date(b.cmsFileCreatedDate).getTime()
      : new Date(b.cmsFileCreatedDate).getTime() -
        new Date(a.cmsFileCreatedDate).getTime();

  return cmsDocuments.sort(compareFunction);
};

export const sortAscendingByDocumentTypeAndCreationDate = <
  T extends PresentationDocumentProperties
>(
  cmsDocuments: T[]
) => {
  const compareFunction = (a: T, b: T) =>
    a.cmsDocType.documentType === b.cmsDocType.documentType
      ? new Date(a.cmsFileCreatedDate).getTime() -
        new Date(b.cmsFileCreatedDate).getTime()
      : customSortByDocumentType(
          a.cmsDocType.documentType,
          b.cmsDocType.documentType
        );

  return cmsDocuments.sort(compareFunction);
};

export const sortAscendingByListOrderAndId = <
  T extends PresentationDocumentProperties
>(
  cmsDocuments: T[]
) => {
  const compareFunction = (a: T, b: T) =>
    a.categoryListOrder &&
    b.categoryListOrder &&
    a.categoryListOrder !== b.categoryListOrder
      ? a.categoryListOrder < b.categoryListOrder
        ? -1
        : 1
      : a.documentId < b.documentId
      ? -1
      : 1;

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

const unusedCommRegexes = [
  //UM, must be standalone word
  /(?<=^|\s)UM(?=\s|$)/gi,
  //UM+digit, must be standalone word,
  /(?<=^|\s)UM\d+(?=\s|$)/gi,
  //UNUSED, standalone word,
  /(?<=^|\s)UNUSED(?=\s|$)/gi,
  //UNUSED+digit, standalone word,
  /(?<=^|\s)UNUSED\d+(?=\s|$)/gi,
  //-UM, as an ending/suffix to a word
  /(?<=^|\s)\S+-UM(?=\s|$)/gi,
  //-UM+digits, as an ending/suffix to a word
  /(?<=^|\s)\S+-UM\d+(?=\s|$)/gi,
  //MG6C, standalone word
  /(?<=^|\s)MG6C(?=\s|$)/gi,
  //MG6D, standalone word
  /(?<=^|\s)MG6D(?=\s|$)/gi,
  //MG6E, standalone word
  /(?<=^|\s)MG6E(?=\s|$)/gi,
  //MG06C, standalone word
  /(?<=^|\s)MG06C(?=\s|$)/gi,
  //MG06D, standalone word
  /(?<=^|\s)MG06D(?=\s|$)/gi,
  //MG06E, standalone word
  /(?<=^|\s)MG06E(?=\s|$)/gi,
  //SDC, standalone word
  /(?<=^|\s)SDC(?=\s|$)/gi,
];

export const isUnusedCommunicationMaterial = (
  filename: string,
  documentTypeId: CmsDocType["documentTypeId"]
) =>
  !!filename &&
  documentTypeId === 1029 &&
  !filename.includes("UM/") &&
  unusedCommRegexes.some((regex) => filename.match(regex));

export const getCommunicationsSubCategory = <
  T extends PresentationDocumentProperties
>(
  doc: T
): CommunicationSubCategory =>
  doc.cmsOriginalFileName?.endsWith(".hte")
    ? CommunicationSubCategory.emails
    : CommunicationSubCategory.communicationFiles;

export const getDocumentAttachments = <
  T extends PresentationDocumentProperties
>(
  item: T,
  docs: T[]
) =>
  item.cmsOriginalFileName?.endsWith(".hte")
    ? docs
        .filter((doc) => doc.parentDocumentId === item.documentId)
        .map(({ documentId, presentationTitle }) => ({
          documentId: documentId,
          name: presentationTitle,
        }))
    : [];
