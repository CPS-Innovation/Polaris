import {
  isAlreadyReportedDocument,
  addToReportedDocuments,
} from "../utils/reportDocuments";

describe("reportDocuments utils", () => {
  it("Should be able to add document id to the reported list", () => {
    expect(isAlreadyReportedDocument("1")).toBe(false);
    addToReportedDocuments("1");
    expect(isAlreadyReportedDocument("1")).toBe(true);
  });
});
