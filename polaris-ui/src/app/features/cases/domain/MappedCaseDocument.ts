import { PresentationDocumentProperties } from "./PdfDocument";

export type MappedCaseDocument = PresentationDocumentProperties & {
  tabSafeId: string;
  presentationCategory: string;
  presentationFileName: string;
};
