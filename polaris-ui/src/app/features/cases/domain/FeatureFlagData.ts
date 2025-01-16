declare global {
  interface Window {
    Cypress: any;
  }
}

export type FeatureFlagData = {
  redactionLog: boolean;
  fullScreen: boolean;
  notes: boolean;
  searchPII: boolean;
  renameDocument: boolean;
  reclassify: boolean;
  externalRedirectCaseReviewApp: boolean;
  externalRedirectBulkUmApp: boolean;
  pageDelete: boolean;
  pageRotate: boolean;
  notifications: boolean;
  isUnused: boolean;
};

export type FeatureFlagQueryParams = {
  redactionLog: string;
  fullScreen: string;
  notes: string;
  searchPII: string;
  renameDocument: string;
  reclassify: string;
  externalRedirectCaseReviewApp: string;
  externalRedirectBulkUmApp: string;
  pageDelete: string;
  pageRotate: string;
  notifications: string;
  isUnused: string;
};
