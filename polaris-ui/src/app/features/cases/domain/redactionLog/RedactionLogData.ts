import { AreaDivision } from "./AreaDivision";
import { RedactionLogDocumentType } from "./RedactionLogDocumentType";
import { RedactionType } from "./RedactionType";
import { InvestigatingAgency } from "./InvestigatingAgency";
import { ListItem, ListItemWithoutChildren } from "./ListItem";

export interface AreasOrDivisionData extends ListItem {
  name: AreaDivision;
}

export interface RedactionLogDocumentTypeData extends ListItem {
  name: RedactionLogDocumentType;
}

export interface InvestigatingAgencyData extends ListItem {
  name: InvestigatingAgency;
}

export interface RedactionTypeDataWithChildren extends ListItem {
  name: RedactionType;
}

export interface RedactionTypeData extends ListItemWithoutChildren {
  id: string;
  name: RedactionType;
}
export interface RedactionLogData {
  areas: AreasOrDivisionData[];
  divisions: AreasOrDivisionData[];
  documentTypes: RedactionLogDocumentTypeData[];
  investigatingAgencies: InvestigatingAgencyData[];
  redactionTypes: RedactionTypeDataWithChildren[];
}
