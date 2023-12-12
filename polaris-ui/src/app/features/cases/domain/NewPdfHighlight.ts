import { NewHighlight } from "../../../../react-pdf-highlighter";
import { RedactionTypeData } from "../domain/redactionLog/RedactionLogData";
//TODO: Split redaction highlight and searchHighlight to have its own unique properties
export interface NewPdfHighlight extends NewHighlight {
  type: "search" | "redaction";
  textContent?: string;
  redactionType?: RedactionTypeData;
}
