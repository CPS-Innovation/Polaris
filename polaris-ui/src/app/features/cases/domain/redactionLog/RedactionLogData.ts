import { ListItem } from "./ListItem";

export interface RedactionTypeData {
  id: string;
  name: string;
}
export interface RedactionLogData {
  areas: ListItem[];
  divisions: ListItem[];
  documentTypes: ListItem[];
  investigatingAgencies: ListItem[];
  missedRedactions: ListItem[];
}
