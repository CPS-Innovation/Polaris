import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";

export const addDocumentNameMatches = (
  mappedTextSearchResult: MappedTextSearchResult,
  documentNameMatches: MappedDocumentResult[]
): MappedTextSearchResult => {

  documentNameMatches.map(documentNameMatch => {
    const documentIndex = mappedTextSearchResult.documentResults.findIndex(document => document.documentId === documentNameMatch.documentId);
    if (documentIndex) {
      mappedTextSearchResult.documentResults[documentIndex].isDocumentNameMatch = true;
    } else {
      mappedTextSearchResult.documentResults.splice(0, 0, documentNameMatch);
    }
  })
  return {
    ...mappedTextSearchResult
  };
};


