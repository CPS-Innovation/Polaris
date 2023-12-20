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
export interface RedactionLogData {
  areas: ListItem[];
  divisions: ListItem[];
  documentTypes: ListItem[];
  investigatingAgencies: ListItem[];
  missedRedactions: ListItem[];
  ouCodeMapping: OuCodeMapping[];
}
