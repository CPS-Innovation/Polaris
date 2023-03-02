import { PresentationDocumentProperties } from "./PipelineDocument";

export type MappedCaseDocument = PresentationDocumentProperties & {
  tabSafeId: string;
  presentationCategory: string;
  presentationFileName: string;
};
