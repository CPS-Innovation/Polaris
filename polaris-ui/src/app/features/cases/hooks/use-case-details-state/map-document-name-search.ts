import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";

type TDocument = MappedTextSearchResult["documentResults"][number];

export const mapDocumentNameSearch = (
  searchTerm: string,
  caseDocuments: MappedCaseDocument[],
): MappedTextSearchResult => {

  const baseCaseDocuments = caseDocuments.filter(document => document.presentationTitle.toLowerCase().includes(searchTerm.toLowerCase()));

  const documentResults: TDocument[] = baseCaseDocuments.map(document => ({
    ...document,
    occurrencesInDocumentCount: 0,
    occurrences: [],
    isVisible: true,
    isDocumentNameMatch: true,
  }));

  return {
    totalOccurrencesCount: documentResults.length,
    filteredOccurrencesCount: documentResults.length,
    filteredDocumentCount: documentResults.length,
    documentResults,
  };
};

