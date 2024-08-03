export type MaterialType = {
  code: string;
  description: string;
  longDescription: string;
  classification: Omit<ReclassifyVariant, "IMMEDIATE">;
  addAsUsedOrUnused?: "Y";
};

export type ReclassifyVariant = "OTHER" | "EXHIBIT" | "STATEMENT" | "IMMEDIATE";
