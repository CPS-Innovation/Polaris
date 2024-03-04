import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { AccordionDocumentSection } from "../../presentation/case-details/accordion/types";
import {
  sortDocumentsByCreatedDate,
  sortAscendingByDocumentTypeAndCreationDate,
  sortAscendingByListOrderAndId,
  isUnusedCommunicationMaterial,
  getCommunicationsSubCategory,
} from "./document-category-helpers";

export enum CommunicationSubCategory {
  emails = "Emails",
  communicationFiles = "Communication files",
}

const docTypeTest = (
  caseDocument: PresentationDocumentProperties,
  ids: number[]
) =>
  !!caseDocument.cmsDocType.documentTypeId &&
  ids.some((id) => id === caseDocument.cmsDocType.documentTypeId);

const documentCategoryDefinitions: {
  category: string;
  showIfEmpty: boolean;
  testFn: (caseDocument: PresentationDocumentProperties) => boolean;
  sortFn: (
    caseDocuments: PresentationDocumentProperties[]
  ) => PresentationDocumentProperties[];
  subCategoryFn?: (caseDocument: PresentationDocumentProperties) => string;
}[] = [
  // todo: when we know, write the `test` logic to identify which document goes in which section
  {
    category: "Reviews",
    showIfEmpty: true,
    testFn: (doc) =>
      // todo: PCD are artificial documents, write a unit test for this
      doc.cmsDocType.documentType === "PCD" ||
      docTypeTest(doc, [101, 102, 103, 104, 189, 212, 227, 1034, 1035, 1064]),
    sortFn: sortDocumentsByCreatedDate,
  },
  {
    category: "Case overview",
    showIfEmpty: true,
    testFn: (doc) =>
      docTypeTest(
        doc,
        [1002, 1003, 1004, 1005, 1006, 1036, 1037, 1038, 1060, 1061, 225887]
      ),
    sortFn: sortAscendingByDocumentTypeAndCreationDate,
  },
  {
    category: "Statements",
    showIfEmpty: true,
    testFn: (doc) =>
      doc.cmsDocType.documentCategory !== "UnusedStatement" &&
      doc.cmsDocType.documentCategory !== "Unused" &&
      docTypeTest(doc, [1031, 1059]),
    sortFn: sortAscendingByListOrderAndId,
  },
  {
    category: "Exhibits",
    showIfEmpty: true,
    testFn: (doc) =>
      docTypeTest(
        doc,
        [
          1019, 1020, 1028, 1030, 1042, 1044, 1050, 1062, 1066, 1201, 100239,
          225569, 226148,
        ]
      ),
    sortFn: sortAscendingByListOrderAndId,
  },
  {
    category: "Forensics",
    showIfEmpty: true,
    testFn: (doc) => docTypeTest(doc, [1027, 1048, 1049, 1203]),
    sortFn: sortDocumentsByCreatedDate,
  },
  {
    category: "Unused material",
    showIfEmpty: true,
    testFn: (doc) =>
      isUnusedCommunicationMaterial(
        doc.presentationTitle,
        doc.cmsDocType.documentTypeId
      ) ||
      doc.cmsDocType.documentCategory === "UnusedStatement" ||
      doc.cmsDocType.documentCategory === "Unused" ||
      docTypeTest(doc, [1001, 1008, 1009, 1010, 1011, 1039, 1202]),
    sortFn: sortAscendingByDocumentTypeAndCreationDate,
  },
  {
    category: "Defendant",
    showIfEmpty: true,
    testFn: (doc) =>
      docTypeTest(doc, [1056, 1057, 1058, 225357, 225638, 225654]),
    sortFn: sortDocumentsByCreatedDate,
  },
  {
    category: "Court preparation",
    showIfEmpty: true,
    testFn: (doc) =>
      docTypeTest(
        doc,
        [
          82, 86, 87, 106, 107, 135, 187, 516, 1012, 1013, 1014, 1015, 1024,
          1025, 1033, 1040, 1041, 1045, 1046, 1047, 1063, 225902, 225911,
          225917, 225933, 225944, 226379, 226558,
        ]
      ),
    sortFn: sortAscendingByDocumentTypeAndCreationDate,
  },
  {
    category: "Communications",
    showIfEmpty: true,
    testFn: (doc) =>
      doc.cmsOriginalFileExtension === ".hte" ||
      docTypeTest(
        doc,
        [
          1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
          21, 22, 23, 24, 25, 26, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42,
          43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
          60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76,
          77, 78, 79, 80, 81, 83, 84, 85, 88, 89, 91, 97, 98, 99, 100, 105, 108,
          109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122,
          123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 136, 137,
          138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151,
          152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165,
          166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
          180, 181, 182, 183, 184, 185, 186, 188, 190, 191, 192, 193, 194, 195,
          196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
          210, 211, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224,
          225, 226, 228, 229, 230, 231, 232, 511, 512, 513, 514, 515, 517, 1007,
          1026, 1029, 1032, 1055, 1065, 1200, 100230, 100231, 100232, 100233,
          100234, 100235, 100236, 100237, 100238, 100240, 100241, 100242,
          100243, 100244, 100245, 100246, 100247, 100248, 100249, 100250,
          100251, 100252, 100253, 225581, 225582, 225583, 225584, 226015,
        ]
      ),
    sortFn: sortDocumentsByCreatedDate,
    subCategoryFn: getCommunicationsSubCategory,
  },
  // have Uncategorised last so it can scoop up any unmatched documents
  {
    category: "Uncategorised",
    showIfEmpty: false,
    testFn: (doc) =>
      docTypeTest(doc, [1051, 1052, 1053, 1054]) ||
      // match to Uncategorised if we have got this far
      // todo: add AppInsights logging for every time we find ourselves here
      true,
    sortFn: sortDocumentsByCreatedDate,
  },
];

export const categoryNamesInPresentationOrder = documentCategoryDefinitions.map(
  ({ category }) => category
);

export const getCategory = (item: PresentationDocumentProperties) => {
  let subCategory = null;
  const category = documentCategoryDefinitions.find(({ testFn: test }) =>
    test(item)
  )!.category;
  const categoryDef = documentCategoryDefinitions.find(
    ({ category: categoryName }) => categoryName === category
  )!;
  if (categoryDef.subCategoryFn) {
    subCategory = categoryDef.subCategoryFn(item);
  }
  return {
    category,
    subCategory,
  };
};

export const getCategorySort = (item: AccordionDocumentSection) =>
  documentCategoryDefinitions.find(
    ({ category }) => category === item.sectionId
  )!.sortFn;
