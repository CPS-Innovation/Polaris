/*
 * @jest-environment node
 */

import { getCategory } from "./document-category-definitions";
import path from "path";
import fs from "fs";
import * as csv from "fast-csv";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

type Row = { docTypeId: string; category: string };

describe("documentCategoryDefinitions", () => {
  let rows: Row[] = [];

  beforeAll((done) => {
    fs.createReadStream(
      path.resolve(__dirname, "document-category-definitions.test.csv")
    )
      .pipe(
        csv.parse<Row, Row>({
          renameHeaders: true,
          headers: ["docTypeId", "category"],
        })
      )
      .on("data", (row) => rows.push(row))
      .on("end", () => done());
  });

  it("every doctype in the csv file maps to the expected category", () => {
    rows.forEach(({ docTypeId, category }) => {
      const categoryResult = getCategory({
        cmsDocType: { documentTypeId: parseInt(docTypeId, 10) },
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
