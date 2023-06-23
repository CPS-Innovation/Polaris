export type ApiTextSearchResult = {
  id: string;
  caseId: number;
  polarisDocumentId?: string;
  documentId: string;
  pageIndex: number;
  lineIndex: number;
  pageHeight: number;
  pageWidth: number;
  text: string;
  words: {
    boundingBox: number[] | null;
    matchType: "None" | "Exact" | "Fuzzy";
    text: string;
    confidence: number;
  }[];
};
