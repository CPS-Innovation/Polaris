import { ChargeStatus } from "./ChargeStatus";
import { RedactionCategory } from "./RedactionCategory";

export interface CreateRequest {
  urn: string;
  unit: {
    id: string;
    type: string;
    areaDivisionName: string;
    name: string;
  };
  investigatingAgency: {
    id: string;
    name: string;
  };
  documentType: {
    id: string;
    name: string;
  };
  missedRedaction: {
    id: string;
    name: string;
  };
  notes: string | null;
  returnedToInvestigativeAuthority: boolean;
  chargeStatus: ChargeStatus;
  redactionType: RedactionCategory;
}
