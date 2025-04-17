import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";

export const combineDocumentNameMatches = (
  mappedTextSearchResult: MappedTextSearchResult,
  documentNameMatches: MappedDocumentResult[],
  documentNameSearchFeatureEnabled: boolean
): MappedTextSearchResult => {
  if (!documentNameSearchFeatureEnabled) {
    return mappedTextSearchResult;
  }

  documentNameMatches.forEach((documentNameMatch) => {
    const documentIndex = mappedTextSearchResult.documentResults.findIndex(
      (document) => document.documentId === documentNameMatch.documentId
    );
    if (documentIndex !== -1) {
      mappedTextSearchResult.documentResults[
        documentIndex
      ].isDocumentNameMatch = true;
      mappedTextSearchResult.totalOccurrencesCount += 1;
      mappedTextSearchResult.filteredOccurrencesCount += 1;
    } else {
      mappedTextSearchResult.documentResults.splice(0, 0, documentNameMatch);
      mappedTextSearchResult.filteredDocumentCount += 1;
      mappedTextSearchResult.totalOccurrencesCount += 1;
      mappedTextSearchResult.filteredOccurrencesCount += 1;
    }
  });

  return {
    ...mappedTextSearchResult,
  };
};
