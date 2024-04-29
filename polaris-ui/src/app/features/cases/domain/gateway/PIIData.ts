export type PIISearchResult = {
  text: string;
  context: string;
};

export type PIIData = {
  show: boolean;
  documentId: string;
  piiSearchResult: PIISearchResult[];
  piiDataStatus: "failure" | "polling" | "success" | "initial";
};
