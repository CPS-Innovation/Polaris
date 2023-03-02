import { CombinedState } from "../../domain/CombinedState";
import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";

type SortFn = (a: MappedDocumentResult, b: MappedDocumentResult) => 1 | 0 | -1;

const SortByPropertyDesc = (
  property: Extract<
    keyof MappedDocumentResult,
    "documentId" | "cmsFileCreatedDate" | "occurrencesInDocumentCount"
  >,
  a: MappedDocumentResult,
  b: MappedDocumentResult
) => {
  return a[property]! < b[property]! ? 1 : a[property]! > b[property]! ? -1 : 0;
};

// Sorting on created date then by document id gives deterministic results
const sortByDateDesc: SortFn = (a, b) =>
  // sort by created date desc
  SortByPropertyDesc("cmsFileCreatedDate", a, b) ||
  // then by documentId
  SortByPropertyDesc("documentId", a, b);

// Sorting on occurrences then by created date then by document id gives
//  deterministic results, but also means that if the user is toggling between
//  the sort orders and all documents have, say, only one occurrence, we don't
//  get the order arbitrarily flipping about
const sortByOccurancesPerDocumentDesc: SortFn = (a, b) =>
  // sort by occurrences desc
  SortByPropertyDesc("occurrencesInDocumentCount", a, b) ||
  // sort by created date desc
  SortByPropertyDesc("cmsFileCreatedDate", a, b) ||
  // then by documentId
  SortByPropertyDesc("documentId", a, b);

export const sortMappedTextSearchResult = (
  mappedTextSearchResult: MappedTextSearchResult,
  sortOrder: CombinedState["searchState"]["resultsOrder"]
): MappedTextSearchResult => {
  return {
    ...mappedTextSearchResult,
    documentResults: [
      ...mappedTextSearchResult.documentResults
        .slice() // make sure we do not return the same reference as the incoming array
        .sort(
          sortOrder === "byDateDesc"
            ? sortByDateDesc
            : sortByOccurancesPerDocumentDesc
        ),
    ],
  };
};
