import { ListItemWithoutChildren } from "./ListItem";
import { ChargeStatus } from "./ChargeStatus";
import { RedactionCategory } from "./RedactionCategory";
import { RedactionTypeData } from "./RedactionLogData";

export type RedactionLogRequestData = {
  urn: string;
  unit: {
    id: string;
    type: "Area";
    areaDivisionName: string;
    name: string;
  };
  investigatingAgency: ListItemWithoutChildren;
  documentType: ListItemWithoutChildren;
  missedRedactions: RedactionTypeData[];
  chargeStatus: ChargeStatus;
  redactionType: RedactionCategory;
  notes: string | null;
  returnedToInvestigativeAuthority: boolean;
};
