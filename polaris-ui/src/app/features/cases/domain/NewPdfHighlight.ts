import { NewHighlight } from "../../../../react-pdf-highlighter";
//TODO: Split redaction highlight and searchHighlight to have its own unique properties
export interface NewPdfHighlight extends NewHighlight {
  type: "search" | "redaction";
  textContent?: string;
  redactionType?: string;
}
