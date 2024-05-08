export type SearchPIIDataItem = {
  text: string;
  context: string;
};

export type SearchPIIData = {
  show: boolean;
  documentId: string;
  searchPIIResult: SearchPIIDataItem[];
  getSearchPIIStatus: "failure" | "polling" | "success" | "initial";
};
