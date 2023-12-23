import { NewHighlight } from "../../../../react-pdf-highlighter";
import { RedactionTypeData } from "../domain/redactionLog/RedactionLogData";
export interface NewPdfHighlight extends NewHighlight {
  type: "search" | "redaction";
  textContent?: string;
  redactionType?: RedactionTypeData;
}
