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
  externalRedirect: boolean;
  pageDelete: boolean;
  pageRotate: boolean;
  notifications: boolean;
};

export type FeatureFlagQueryParams = {
  redactionLog: string;
  fullScreen: string;
  notes: string;
  searchPII: string;
  renameDocument: string;
  reclassify: string;
  externalRedirect: string;
  pageDelete: string;
  pageRotate: string;
  notifications: string;
};
