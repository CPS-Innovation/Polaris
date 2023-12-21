import { ListItem } from "./ListItem";

export interface RedactionTypeData {
  id: string;
  name: string;
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
  missedRedactions: ListItem[];
  ouCodeMapping: OuCodeMapping[];
}

export interface RedactionLogMappingData {
  areaMapping: { ou: string; areaId: string | null; unitId: string | null }[];
  docTypeMapping: { cmdDocTypeId: string; docTypeId: string }[];
  iAMapping: { ou: string; ia: string }[];
}
