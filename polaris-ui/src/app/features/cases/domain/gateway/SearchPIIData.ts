import { ISearchPIIHighlight } from "../NewPdfHighlight";

export type SearchPIIResultItem = {
  text: string;
  fileName: string;
  id: string;
  lineIndex: number;
  pageHeight: number;
  pageIndex: number;
  pageWidth: number;
  polarisDocumentId: string;
  words: {
    boundingBox: number[] | null;
    matchType: "None" | "Exact" | "Fuzzy";
    piiCategory: string;
    text: string;
  }[];
};

export type SearchPIIData = {
  show: boolean;
  documentId: string;
  polarisDocumentVersionId: number;
  searchPIIHighlights: ISearchPIIHighlight[];
  getSearchPIIStatus: "failure" | "success" | "loading" | "initial";
};
