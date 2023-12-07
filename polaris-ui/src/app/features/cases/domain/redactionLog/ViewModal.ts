import { ListItem } from "./ListItem";
import { ChargeStatus } from "./ChargeStatus";
import { RedactionCategory } from "./RedactionCategory";
import { CreateRequest } from "./CreateRequest";
import { AreaDivision } from "./AreaDivision";
import { RedactionTypes } from "./RedactionLogData";
export type ListItemWithoutChildren = Omit<ListItem, "children">;

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
  missedRedactions: RedactionTypes[];
  chargeStatus: ChargeStatus;
  redactionType: RedactionCategory;
  notes: string | null;
  returnedToInvestigativeAuthority: boolean;
};
