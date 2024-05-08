declare global {
  interface Window {
    Cypress: any;
  }
}

export type FeatureFlagData = {
  redactionLog: boolean;
  fullScreen: boolean;
  notes: boolean;
};

export type FeatureFlagQueryParams = {
  redactionLog: string;
  fullScreen: string;
  notes: string;
};
