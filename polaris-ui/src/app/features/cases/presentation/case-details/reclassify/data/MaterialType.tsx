export type MaterialType = {
  typeId: number;
  description: string;
  newClassificationVariant: ReclassifyVariant;
};

export type ReclassifyVariant = "Other" | "Exhibit" | "Statement" | "Immediate";
