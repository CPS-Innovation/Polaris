/*
 * @jest-environment node
 */

import { getCategory } from "./document-category-definitions";
import path from "path";
import fs from "fs";
import * as csv from "fast-csv";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

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

      expect({ docTypeId, category: categoryResult }).toEqual({
        docTypeId,
        category,
      });
    });
  });

  it("can resolve a PCD document to Reviews", () => {
    const result = getCategory({
      cmsDocType: { documentType: "PCD" },
    } as PresentationDocumentProperties);

    expect(result).toBe("Reviews");
  });

  it("can resolve any document with docTypeCategory as 'Unused' into 'Unused material' accordion category", () => {
    const result = getCategory({
      cmsDocType: { documentCategory: "Unused" },
    } as PresentationDocumentProperties);

    expect(result).toBe("Unused material");
  });

  it("can resolve any document with docTypeCategory as 'UnusedStatement' into 'Unused material' accordion category", () => {
    const result = getCategory({
      cmsDocType: { documentCategory: "UnusedStatement" },
    } as PresentationDocumentProperties);

    expect(result).toBe("Unused material");
  });

  it("will only resolve documents with correct statement documentTypeId and docTypeCategory not equal to 'UnusedStatement' or 'Unused' into 'Statements' accordion category", () => {
    const result = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "UsedStatement" },
    } as PresentationDocumentProperties);

    expect(result).toBe("Statements");

    const result1 = getCategory({
      cmsDocType: { documentTypeId: 1059, documentCategory: null as any },
    } as PresentationDocumentProperties);

    expect(result1).toBe("Statements");

    const result3 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "UnusedStatement" },
    } as PresentationDocumentProperties);

    expect(result3).toBe("Unused material");

    const result4 = getCategory({
      cmsDocType: { documentTypeId: 1031, documentCategory: "Unused" },
    } as PresentationDocumentProperties);

    expect(result4).toBe("Unused material");
  });

  it("can resolve a documents category to Uncategorised if no prior categories match", () => {
    const result = getCategory({
      cmsDocType: {},
    } as PresentationDocumentProperties);

    expect(result).toBe("Uncategorised");
  });

  it("can resolve an unknown category to Uncategorised if no prior categories match", () => {
    const result = getCategory({
      cmsDocType: { documentTypeId: -1, documentType: "Unknown" },
    } as PresentationDocumentProperties);

    expect(result).toBe("Uncategorised");
  });
});
