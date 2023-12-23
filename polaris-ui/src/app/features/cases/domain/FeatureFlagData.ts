declare global {
  interface Window {
    Cypress: any;
  }
}

export type FeatureFlagData = {
  redactionLog: boolean;
};

export type FeatureFlagQueryParams = {
  redactionLog: string;
};
