import { ListItem } from "./ListItem";

export interface RedactionTypeData {
  id: string;
  name: string;
  isDeletedPage?: boolean;
}

export interface OuCodeMapping {
  ouCode: string;
  areaCode: string;
  areaName: string;
  investigatingAgencyCode: string;
  investigatingAgencyName: string;
}
export interface RedactionLogLookUpsData {
  areas: ListItem[];
  divisions: ListItem[];
  documentTypes: ListItem[];
  investigatingAgencies: ListItem[];
  missedRedactions: RedactionTypeData[];
  ouCodeMapping: OuCodeMapping[];
}

export interface RedactionLogMappingData {
  businessUnits: {
    ou: string;
    areaId: string | null;
    unitId: string | null;
  }[];
  documentTypes: { cmsDocTypeId: string; docTypeId: string }[];
  investigatingAgencies: {
    ouCode: string;
    investigatingAgencyId: string | null;
  }[];
}
