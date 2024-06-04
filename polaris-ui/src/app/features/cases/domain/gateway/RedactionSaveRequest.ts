import { RedactionSavePage } from "./RedactionSavePage";

export type RedactionSaveData = {
  documentId: string;
  redactions: RedactionSavePage[];
};

export type PIIAnalyticsData = {
  categories?: {
    polarisCategory: string;
    providerCategory: string;
    countSuggestions: number;
    countAccepted: number;
    countIgnored: number;
    countAmended: number;
  }[];
};

export type RedactionSaveRequest = RedactionSaveData & {
  pii?: PIIAnalyticsData;
};
