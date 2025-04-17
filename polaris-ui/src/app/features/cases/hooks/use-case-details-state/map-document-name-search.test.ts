import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { mapDocumentNameSearch } from "./map-document-name-search";

describe("mapDocumentNameSearch", () => {
  it("can map document name matches to a MappedTextSearchResult", () => {
    const searchTerm = "abc";
    const caseDocuments = [
      { documentId: "1", presentationTitle: "testabc" },
      { documentId: "2", presentationTitle: "testxyz" },
    ] as MappedCaseDocument[];

    const result = mapDocumentNameSearch(searchTerm, caseDocuments);

    expect(result).toEqual({
      documentResults: [
        {
          documentId: "1",
          isVisible: true,
          isDocumentNameMatch: true,
          presentationTitle: "testabc",
          occurrences: [],
          occurrencesInDocumentCount: 0,
        },
      ],
      filteredDocumentCount: 1,
      filteredOccurrencesCount: 1,
      totalOccurrencesCount: 1,
    });
  });
});
