import { NewHighlight } from "../../../../react-pdf-highlighter";

export interface NewPdfHighlight extends NewHighlight {
  type: "search" | "redaction";
  textContent?: string;
}
