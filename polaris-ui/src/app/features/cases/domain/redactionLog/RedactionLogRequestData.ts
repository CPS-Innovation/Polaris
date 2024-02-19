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
  redactions: {
    missedRedaction: RedactionTypeData;
    redactionType: RedactionCategory;
    returnedToInvestigativeAuthority: boolean;
  }[];
  chargeStatus: ChargeStatus;
  notes: string | null;
  cmsValues: {
    documentId: string;
    originalFileName: string;
    documentTypeId: number;
    documentType: string;
    fileCreatedDate: string;
  };
};
