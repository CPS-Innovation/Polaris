import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";
import { combineDocumentNameMatches } from "./combine-document-name-matches";

describe("combineDocumentNameMatches", () => {
  it("can combine document name matches with content matches and return a MappedTextSearchResult", () => {
    const mappedTextSearchResult = {
      documentResults: [
        {
          documentId: "1",
          isVisible: true,
          isDocumentNameMatch: false,
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
                {
                  isHighlighted: false,
                  text: "test",
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
          documentId: "2",
          isVisible: true,
          isDocumentNameMatch: false,
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
    } as MappedTextSearchResult;

    const documentNameMatches = [
      { documentId: "1", presentationTitle: "testabc" },
    ] as MappedDocumentResult[];
    const documentNameSearchFeatureEnabled = true;

    const result = combineDocumentNameMatches(
      mappedTextSearchResult,
      documentNameMatches,
      documentNameSearchFeatureEnabled
    );

    expect(result).toEqual({
      documentResults: [
        {
          documentId: "1",
          isVisible: true,
          isDocumentNameMatch: true,
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
                {
                  isHighlighted: false,
                  text: "test",
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
          documentId: "2",
          isVisible: true,
          isDocumentNameMatch: false,
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
      filteredOccurrencesCount: 6,
      totalOccurrencesCount: 6,
    });
  });
});
