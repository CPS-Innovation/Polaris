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

// the redaction log service's source client id for this app
export const CASEWORK_APP_SOURCE_CLIENT_ID = 1;
