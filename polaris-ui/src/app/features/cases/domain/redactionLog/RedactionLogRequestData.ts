import { ListItemWithoutChildren } from "./ListItem";
import { ChargeStatus } from "./ChargeStatus";
import { RedactionCategory } from "./RedactionCategory";
import { AreaDivision } from "./AreaDivision";
import { RedactionTypeData } from "./RedactionLogData";

export type RedactionLogRequestData = {
  urn: string;
  unit: {
    id: string;
    type: "Area";
    areaDivisionName: AreaDivision;
    name: AreaDivision;
  };
  investigatingAgency: ListItemWithoutChildren;
  documentType: ListItemWithoutChildren;
  missedRedactions: RedactionTypeData[];
  chargeStatus: ChargeStatus;
  redactionType: RedactionCategory;
  notes: string | null;
  returnedToInvestigativeAuthority: boolean;
};
