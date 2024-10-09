import { RedactionSavePage } from "./RedactionSavePage";

export type RedactionSaveData = {
  documentId: string;
  redactions: RedactionSavePage[];
  documentModifications: {
    pageIndex: number;
    operation: "delete";
  }[];
};

export type PIIAnalyticsData = {
  categories?: {
    polarisCategory: string;
    providerCategory: string;
    countSuggestions: number;
    countAccepted: number;
    countAmended: number;
  }[];
};

export type RedactionSaveRequest = RedactionSaveData & {
  pii?: PIIAnalyticsData;
};
