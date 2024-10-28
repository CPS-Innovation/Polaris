import { ListItemWithoutChildren } from "./ListItem";
import { ChargeStatus } from "./ChargeStatus";
import { RedactionCategory } from "./RedactionCategory";
import { RedactionTypeData } from "./RedactionLogData";
import { CmsDocType } from "../gateway/CmsDocType";

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
    documentId: number;
    originalFileName: string;
    documentTypeId: CmsDocType["documentTypeId"];
    documentType: string;
    fileCreatedDate: string;
  };
};
