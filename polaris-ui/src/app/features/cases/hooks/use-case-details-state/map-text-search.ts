import { ApiTextSearchResult } from "../../domain/ApiTextSearchResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";

type TDocument = MappedTextSearchResult["documentResults"][number];

export const mapTextSearch = (
  apiResults: ApiTextSearchResult[],
  caseDocuments: MappedCaseDocument[]
): MappedTextSearchResult => {
  let totalOccurrencesCount = 0;

  const apiResultDocuments = apiResults.reduce(
    (accumulator, apiResultDocument) => {
      let documentResult = accumulator.find(
        (mappedResult) =>
          mappedResult.documentId === apiResultDocument.documentId
      );

      if (!documentResult) {
        const baseCaseDocument = caseDocuments.find(
          (caseDocument) =>
            caseDocument.documentId === apiResultDocument.documentId
        );

        documentResult = {
          ...baseCaseDocument,
          occurrencesInDocumentCount: 0,
          occurrences: [],
          isVisible: true,
        } as TDocument;

        accumulator.push(documentResult);
      }

      const { id, pageIndex, words, pageHeight, pageWidth } = apiResultDocument;

      const occurrencesInLine = words
        .filter((word) => word.matchType !== "None")
        // this || clause keeps typescript happy, by this point we are guaranteed to have an array,
        //  with stuff in rather than null, but typescript doen't think so, and I can't find a
        //  type-guard-y kind of way to convince typescript.

        .map(
          ({ boundingBox }) =>
            boundingBox ||
            /* istanbul ignore next */
            []
        );

      const thisOccurrence = {
        id,
        pageIndex,
        pageHeight,
        pageWidth,
        contextTextChunks: words.map((word) => ({
          text: word.text,
          isHighlighted: word.matchType !== "None",
        })),
        occurrencesInLine: occurrencesInLine,
      };

      documentResult.occurrences.push(thisOccurrence);
      documentResult.occurrencesInDocumentCount += occurrencesInLine.length;
      totalOccurrencesCount += occurrencesInLine.length;
      return accumulator;
    },
    [] as MappedDocumentResult[]
  );

  return {
    totalOccurrencesCount: totalOccurrencesCount,
    documentResults: apiResultDocuments,
    filteredOccurrencesCount: totalOccurrencesCount,
    filteredDocumentCount: apiResultDocuments.length,
  };
};
