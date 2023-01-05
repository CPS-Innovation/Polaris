import { MappedDocumentResult } from "./MappedDocumentResult";

export type MappedTextSearchResult = {
  totalOccurrencesCount: number;
  filteredOccurrencesCount: number;
  filteredDocumentCount: number;
  documentResults: MappedDocumentResult[];
};
