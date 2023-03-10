/*
 * @jest-environment node
 */

import { getCategory } from "./document-category-definitions";
import path from "path";
import fs from "fs";
import * as csv from "fast-csv";
import { PresentationDocumentProperties } from "../../domain/PipelineDocument";

type Row = { docType: string; category: string };

describe("documentCategoryDefinitions", () => {
  let rows: Row[] = [];

  beforeAll((done) => {
    fs.createReadStream(
      path.resolve(__dirname, "document-category-definitions.test.csv")
    )
      .pipe(
        csv.parse<Row, Row>({
          renameHeaders: true,
          headers: ["docType", "category"],
        })
      )
      .on("data", (row) => rows.push(row))
      .on("end", () => done());
  });

  it("every doctype in the csv file maps to the expected category", () => {
    rows.forEach(({ docType, category }) => {
      const categoryResult = getCategory({
        cmsDocType: { code: docType },
      } as PresentationDocumentProperties);

      expect({ docType, category: categoryResult }).toEqual({
        docType,
        category,
      });
    });
  });

  it("can resolve a documents category to Uncategorised if no prior categories match", () => {
    const result = getCategory({
      cmsDocType: {},
    } as PresentationDocumentProperties);

    expect(result).toBe("Uncategorised");
  });

  it("can resolve an unknown category to Uncategorised if no prior categories match", () => {
    const result = getCategory({
      cmsDocType: { id: -1, name: "Unknown", code: "Unknown" },
    } as PresentationDocumentProperties);

    expect(result).toBe("Uncategorised");
  });
});
