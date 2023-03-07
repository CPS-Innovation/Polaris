import { PresentationDocumentProperties } from "./PipelineDocument";

export type MappedCaseDocument = PresentationDocumentProperties & {
  presentationCategory: string;
  presentationFileName: string;
};
