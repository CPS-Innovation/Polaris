import { CaseDocument } from "../../domain/CaseDocument";

const docTypeTest = (caseDocument: CaseDocument, codes: string[]) =>
  !!caseDocument.cmsDocType?.code &&
  codes.some(
    (code) =>
      !!code &&
      code.replace(/\s+/g, "") ===
        caseDocument.cmsDocType?.code.replace(/\s+/g, "")
  );

const documentCategoryDefinitions: {
  category: string;
  showIfEmpty: boolean;
  test: (caseDocument: CaseDocument) => boolean;
}[] = [
  // todo: when we know, write the `test` logic to identify which document goes in which section
  {
    category: "Reviews",
    showIfEmpty: true,
    test: (doc) => docTypeTest(doc, ["MG3", "MG3A"]),
  },
  {
    category: "Case overview",
    showIfEmpty: true,
    test: (doc) => docTypeTest(doc, ["MG4", "MG5", "MG6"]),
  },
  {
    category: "Statements",
    showIfEmpty: true,
    test: (doc) => docTypeTest(doc, ["MG9", "MG11", "PE1"]),
  },
  {
    category: "Exhibits",
    showIfEmpty: true,
    test: (doc) => docTypeTest(doc, ["MG12", "Other Exhibit"]),
  },
  {
    category: "Forensics",
    showIfEmpty: true,
    test: (doc) => docTypeTest(doc, ["MG22 SFR"]),
  },
  {
    category: "Unused material",
    showIfEmpty: true,
    test: (doc) =>
      docTypeTest(doc, [
        "MG1",
        "MG6A",
        "MG6B",
        "MG6C",
        "MG6D",
        "MG6E",
        "MG20",
        "MG21",
        "MG21A",
        "PCN3",
        "MG11(R)",
      ]),
  },
  {
    category: "Defendant",
    showIfEmpty: true,
    test: (doc) =>
      docTypeTest(doc, [
        "MG15(ROTI)",
        "MG15(SDN)",
        "MG15(ROVI)",
        "MG15(CNOI)",
        "MG16",
        "MG16(DBCI)",
        "MG16(DDOI)",
        "PE1",
        "PE2",
        "DREP",
        "PCN1",
        "PCN2",
      ]),
  },
  {
    category: "Court preparation",
    showIfEmpty: true,
    test: (doc) =>
      docTypeTest(doc, ["MG2", "MG4B", "MG7", "MG8", "MG10", "MG13", "MG19"]),
  },
  {
    category: "Communications",
    showIfEmpty: true,
    test: (doc) => docTypeTest(doc, ["Other Comm (In)"]),
  },
  // have unknown last so it can scoop up any unmatched documents
  {
    category: "Uncategorised",
    showIfEmpty: false,
    test: () => true,
  },
];

export const categoryNamesInPresentationOrder = documentCategoryDefinitions.map(
  ({ category }) => category
);

export const getCategory = (item: CaseDocument) =>
  documentCategoryDefinitions.find(({ test }) => test(item))!.category;
