import { AreaDivision } from "./AreaDivision";
import { RedactionLogDocType } from "./RedactionLogDocType";
import { RedactionType } from "./RedactionType";
import { ListItem } from "./ListItem";
import { ListItemWithoutChildren } from "./ViewModal";

export interface AreasOrDivision extends ListItem {
  name: AreaDivision;
}

export interface RedactionLogDocumentTypes extends ListItem {
  name: RedactionLogDocType;
}

export interface InvestigatingAgencies extends ListItem {}

export interface RedactionTypes extends ListItemWithoutChildren {
  name: RedactionType;
}

export interface RedactionTypesData extends ListItem {
  name: RedactionType;
}

export interface RedactionLogData {
  areas: AreasOrDivision[];
  divisions: AreasOrDivision[];
  documentTypes: RedactionLogDocumentTypes[];
  investigatingAgencies: InvestigatingAgencies[];
  redactionTypes: RedactionTypes[];
}
