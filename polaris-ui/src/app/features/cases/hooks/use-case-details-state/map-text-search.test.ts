import { ApiTextSearchResult } from "../../domain/ApiTextSearchResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { mapTextSearch } from "./map-text-search";

describe("mapTextSearch", () => {
  it("can map api results to a MappedTextSearchResult", () => {
    const apiResults = [
      {
        id: "1",
        documentId: 1,
        pageIndex: 0,
        words: [
          { boundingBox: [1], text: "foo", confidence: 1, matchType: "Exact" },
          { boundingBox: null, text: "bar", confidence: 1, matchType: "None" },
        ],
      },
      {
        id: "2",
        documentId: 2,
        pageIndex: 0,
        words: [
          { boundingBox: null, text: "baz", confidence: 1, matchType: "None" },
          { boundingBox: [1], text: "foo", confidence: 1, matchType: "Exact" },
        ],
      },
      {
        id: "3",
        documentId: 2,
        pageIndex: 0,
        words: [
          { boundingBox: [1], text: "foo", confidence: 1, matchType: "Fuzzy" },
          { boundingBox: [1], text: "foo", confidence: 1, matchType: "Exact" },
        ],
      },
      {
        id: "4",
        documentId: 2,
        pageIndex: 1,
        words: [
          { boundingBox: [1], text: "foo", confidence: 1, matchType: "Exact" },
          { boundingBox: null, text: "baz", confidence: 1, matchType: "None" },
        ],
      },
    ] as ApiTextSearchResult[];

    const caseDocuments = [
      { documentId: 1 },
      { documentId: 2 },
    ] as MappedCaseDocument[];

    const result = mapTextSearch(apiResults, caseDocuments);

    expect(result).toEqual({
      documentResults: [
        {
          documentId: 1,
          isVisible: true,
          occurrences: [
            {
              contextTextChunks: [
                {
                  isHighlighted: true,
                  text: "foo",
                },
                {
                  isHighlighted: false,
                  text: "bar",
                },
              ],
              id: "1",
              occurrencesInLine: [[1]],
              pageIndex: 0,
            },
          ],
          occurrencesInDocumentCount: 1,
        },
        {
          documentId: 2,
          isVisible: true,
          occurrences: [
            {
              contextTextChunks: [
                {
                  isHighlighted: false,
                  text: "baz",
                },
                {
                  isHighlighted: true,
                  text: "foo",
                },
              ],
              id: "2",
              occurrencesInLine: [[1]],
              pageIndex: 0,
            },
            {
              contextTextChunks: [
                {
                  isHighlighted: true,
                  text: "foo",
                },
                {
                  isHighlighted: true,
                  text: "foo",
                },
              ],
              id: "3",
              occurrencesInLine: [[1], [1]],
              pageIndex: 0,
            },
            {
              contextTextChunks: [
                {
                  isHighlighted: true,
                  text: "foo",
                },
                {
                  isHighlighted: false,
                  text: "baz",
                },
              ],
              id: "4",
              occurrencesInLine: [[1]],
              pageIndex: 1,
            },
          ],
          occurrencesInDocumentCount: 4,
        },
      ],
      filteredDocumentCount: 2,
      filteredOccurrencesCount: 5,
      totalOccurrencesCount: 5,
    });
  });
});
