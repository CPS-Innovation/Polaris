import { NewHighlight } from "../../../../react-pdf-highlighter";
import { RedactionTypeData } from "../domain/redactionLog/RedactionLogData";
export interface NewPdfHighlight extends NewHighlight {
  type: "search" | "redaction" | "searchPII";
  textContent?: string;
  redactionType?: RedactionTypeData;
}

export interface ISearchPIIHighlight extends NewHighlight {
  id: string;
  type: "searchPII";
  textContent?: string;
}
