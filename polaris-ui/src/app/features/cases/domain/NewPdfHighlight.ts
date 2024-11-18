import { NewHighlight } from "../../../../react-pdf-highlighter";
import { RedactionTypeData } from "../domain/redactionLog/RedactionLogData";
export interface NewPdfHighlight extends NewHighlight {
  type: "search" | "redaction" | "searchPII";
  textContent?: string;
  redactionType?: RedactionTypeData;
  searchPIIId?: string;
  content?: any,
  comment?: any
  highlightType?: any
}

export type PIIRedactionStatus =
  | "accepted"
  | "acceptedAll"
  | "ignored"
  | "ignoredAll"
  | "initial";

export interface ISearchPIIHighlight extends NewHighlight {
  id: string;
  type: "searchPII";
  textContent: string;
  redactionStatus: PIIRedactionStatus;
  piiCategory: string;
  redactionType: RedactionTypeData;
  groupId: string;
}
