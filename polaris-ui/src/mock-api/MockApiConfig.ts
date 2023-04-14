export type MockApiConfig = {
  baseUrl: string;
  sourceName: "cypress" | "dev";
  maxDelayMs: number;
  publicUrl: string;
};
