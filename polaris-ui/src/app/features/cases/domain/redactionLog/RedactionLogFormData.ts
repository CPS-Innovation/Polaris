export type UnderRedactionFormData = {
  cpsArea: string;
  businessUnit: string;
  investigatingAgency: string;
  chargeStatus: string;
  documentType: string;
  notes: string;
  returnToIA?: string;
  underRedaction?: string;
  overRedaction?: string;
} & Record<string, string>;
