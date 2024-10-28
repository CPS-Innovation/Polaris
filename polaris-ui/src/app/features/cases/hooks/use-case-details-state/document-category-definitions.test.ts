/*
 * @jest-environment node
 */

import { getCategory } from "./document-category-definitions";
import path from "path";
import fs from "fs";
import * as csv from "fast-csv";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
jest.mock("./document-category-helpers", () => {
  const originalModule = jest.requireActual("./document-category-helpers");
  return { ...originalModule };
});
const docCategoryHelpers = require("./document-category-helpers");

type Row = { docTypeId: string; category: string; docTypeCategory: string };

describe("documentCategoryDefinitions", () => {
  let rows: Row[] = [];

  beforeAll((done) => {
    fs.createReadStream(
      path.resolve(__dirname, "document-category-definitions.test.csv")
    )
      .pipe(
        csv.parse<Row, Row>({
          renameHeaders: true,
          headers: ["docTypeId", "category", "docTypeCategory"],
        })
      )
      .on("data", (row) => rows.push(row))
      .on("end", () => done());
  });

  it("every doctype in the csv file maps to the expected category", () => {
    rows.forEach(({ docTypeId, category, docTypeCategory }) => {
      const categoryResult = getCategory({
        cmsDocType: {
          documentTypeId: parseInt(docTypeId, 10),
          documentCategory: docTypeCategory,
        },
      } as PresentationDocumentProperties);

      expect({ docTypeId, category: categoryResult.category }).toEqual({
        docTypeId,
        category,
      });
    });
  });

  it("can resolve a PCD document to Reviews", () => {
    const result = getCategory({
      cmsDocType: { documentType: "PCD" },
    } as PresentationDocumentProperties);

    expect(result.category).toBe("Reviews");
  });

  it("can resolve any document with docTypeCategory as 'Exhibit' or 'MGForm' or 'OtherForm' and isUnused == true into 'Unused material' accordion category", () => {
    const result1 = getCategory({
      cmsDocType: { documentCategory: "Exhibit" },
      isUnused: true,
    } as PresentationDocumentProperties);

    const result2 = getCategory({
      cmsDocType: { documentCategory: "MGForm" },
      isUnused: true,
    } as PresentationDocumentProperties);

    const result3 = getCategory({
      cmsDocType: { documentCategory: "OtherForm" },
      isUnused: true,
    } as PresentationDocumentProperties);

    expect(result1.category).toBe("Unused material");
    expect(result2.category).toBe("Unused material");
    expect(result3.category).toBe("Unused material");
  });

  it("can resolve any document with docTypeCategory as 'Statement' and isUnused == true into 'Unused material' accordion category", () => {
    const result = getCategory({
      cmsDocType: { documentCategory: "Statement" },
      isUnused: true,
    } as PresentationDocumentProperties);

    expect(result.category).toBe("Unused material");
  });

  it("will only resolve documents with correct statement documentTypeId and docTypeCategory not equal to 'UnusedStatement' or 'Unused' into 'Statements' accordion category", () => {
    const result1 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "Statement" },
    } as PresentationDocumentProperties);

    expect(result1.category).toBe("Statements");

    const result2 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: null as any },
    } as PresentationDocumentProperties);

    expect(result2.category).toBe("Statements");

    const result3 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "Statement" },
      isUnused: true,
    } as PresentationDocumentProperties);

    expect(result3.category).toBe("Unused material");

    const result4 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "Exhibit" },
      isUnused: true,
    } as PresentationDocumentProperties);

    expect(result4.category).toBe("Unused material");
  });

  it("can resolve a documents category to Uncategorised if no prior categories match", () => {
    const result = getCategory({
      cmsDocType: {},
    } as PresentationDocumentProperties);

    expect(result.category).toBe("Uncategorised");
  });

  it("can resolve an unknown category to Uncategorised if no prior categories match", () => {
    const result = getCategory({
      cmsDocType: { documentTypeId: -1, documentType: "Unknown" },
    } as PresentationDocumentProperties);

    expect(result.category).toBe("Uncategorised");
  });

  it(`can resolve document with documentTypeId 1029 under "Unused material", if isUnusedCommunicationMaterial returns true `, () => {
    docCategoryHelpers.isUnusedCommunicationMaterial = () => true;
    const result1 = getCategory({
      cmsDocType: { documentTypeId: 1029 },
      presentationTitle: "UM CM01",
    } as PresentationDocumentProperties);

    const result2 = getCategory({
      cmsDocType: { documentTypeId: 1029 },
      presentationTitle: "CM01 Item 4 a",
    } as PresentationDocumentProperties);

    expect(result1.category).toBe("Unused material");
    expect(result2.category).toBe("Unused material");
    expect(result2.subCategory).toBe(null);
  });

  it(`can resolve document with documentTypeId 1029 under "Communications" category if isUnusedCommunicationMaterial returns false`, () => {
    docCategoryHelpers.isUnusedCommunicationMaterial = () => false;
    const result1 = getCategory({
      cmsDocType: { documentTypeId: 1029 },
      presentationTitle: "CM01",
    } as PresentationDocumentProperties);

    const result2 = getCategory({
      cmsDocType: { documentTypeId: 1029 },
      presentationTitle: " CM01 Typea 4 a",
      cmsOriginalFileName: "foo.hte",
    } as PresentationDocumentProperties);

    expect(result1.category).toBe("Communications");
    expect(result1.subCategory).toBe("Communication files");
    expect(result2.category).toBe("Communications");
    expect(result2.subCategory).toBe("Emails");
  });

  it(`can resolve document  under "Unused material" category if documentCategory is "Statement" and isUnused == true `, () => {
    docCategoryHelpers.isUnusedCommunicationMaterial = () => false;
    const result1 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "Statement" },
      presentationTitle: "CM01",
      isUnused: true,
    } as PresentationDocumentProperties);

    const result2 = getCategory({
      cmsDocType: { documentTypeId: 1059, documentCategory: "Statement" },
      presentationTitle: " CM01 Typea 4 a",
      cmsOriginalFileName: "foo.hte",
      isUnused: true,
    } as PresentationDocumentProperties);

    expect(result1.category).toBe("Unused material");
    expect(result2.category).toBe("Unused material");
  });

  it(`can resolve document  under "Unused material" category if documentCategory is "Unused" `, () => {
    docCategoryHelpers.isUnusedCommunicationMaterial = () => false;
    const result1 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "Exhibit" },
      presentationTitle: "CM01",
      isUnused: true,
    } as PresentationDocumentProperties);

    const result2 = getCategory({
      cmsDocType: { documentTypeId: 1059, documentCategory: "MGForm" },
      presentationTitle: " CM01 Typea 4 a",
      cmsOriginalFileName: "foo.hte",
      isUnused: true,
    } as PresentationDocumentProperties);

    expect(result1.category).toBe("Unused material");
    expect(result2.category).toBe("Unused material");
  });

  it("can resolve document with cmsOriginalFileName ending .hte into 'Communication' category and subCategory 'Emails'", () => {
    const result1 = getCategory({
      cmsDocType: { documentTypeId: -1 },
      cmsOriginalFileName: "foo.hte",
    } as PresentationDocumentProperties);
    expect(result1.category).toBe("Communications");
    expect(result1.subCategory).toBe("Emails");
  });
});
