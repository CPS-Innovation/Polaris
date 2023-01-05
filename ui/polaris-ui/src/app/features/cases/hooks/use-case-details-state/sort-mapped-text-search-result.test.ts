import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";
import { sortMappedTextSearchResult } from "./sort-mapped-text-search-result";

const MAPPED_TEXT_SEARCH_RESULT = {
  documentResults: [
    {
      documentId: 1,
      createdDate: "2022-01-07",
      occurrencesInDocumentCount: 1,
    },
    {
      documentId: 2,
      createdDate: "2022-01-07",
      occurrencesInDocumentCount: 1,
    },
    {
      documentId: 3,
      createdDate: "2022-01-09",
      occurrencesInDocumentCount: 13,
    },
    {
      documentId: 4,
      createdDate: "2022-01-08",
      occurrencesInDocumentCount: 2,
    },
    {
      documentId: 5,
      createdDate: "2022-01-10",
      occurrencesInDocumentCount: 1,
    },
  ],
} as MappedTextSearchResult;

describe("sortMappedTextSearchResult", () => {
  it("can sort documents by date descending", () => {
    const result = sortMappedTextSearchResult(
      MAPPED_TEXT_SEARCH_RESULT,
      "byDateDesc"
    );

    expect(result).toEqual({
      documentResults: [
        {
          createdDate: "2022-01-10",
          documentId: 5,
          occurrencesInDocumentCount: 1,
        },
        {
          createdDate: "2022-01-09",
          documentId: 3,
          occurrencesInDocumentCount: 13,
        },
        {
          createdDate: "2022-01-08",
          documentId: 4,
          occurrencesInDocumentCount: 2,
        },
        {
          createdDate: "2022-01-07",
          documentId: 2,
          occurrencesInDocumentCount: 1,
        },
        {
          createdDate: "2022-01-07",
          documentId: 1,
          occurrencesInDocumentCount: 1,
        },
      ],
    });
  });

  it("can sort documents by occurrences per document descending", () => {
    const result = sortMappedTextSearchResult(
      MAPPED_TEXT_SEARCH_RESULT,
      "byOccurancesPerDocumentDesc"
    );

    expect(result).toEqual({
      documentResults: [
        {
          createdDate: "2022-01-09",
          documentId: 3,
          occurrencesInDocumentCount: 13,
        },
        {
          createdDate: "2022-01-08",
          documentId: 4,
          occurrencesInDocumentCount: 2,
        },
        {
          createdDate: "2022-01-10",
          documentId: 5,
          occurrencesInDocumentCount: 1,
        },
        {
          createdDate: "2022-01-07",
          documentId: 2,
          occurrencesInDocumentCount: 1,
        },
        {
          createdDate: "2022-01-07",
          documentId: 1,
          occurrencesInDocumentCount: 1,
        },
      ],
    });
  });
});
