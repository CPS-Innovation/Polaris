export type MockApiConfig = {
  baseUrl: string;
  sourceName: "cypress" | "dev" | "assurance";
  maxDelayMs: number;
  publicUrl: string;
};
