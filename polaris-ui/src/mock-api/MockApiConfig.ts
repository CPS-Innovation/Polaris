export type MockApiConfig = {
  baseUrl: string;
  sourceName: "cypress" | "dev";
  maxDelayMs: string;
  publicUrl: string;
  redactionLogUrl: string;
};
