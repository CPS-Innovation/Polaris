import { PresentationDocumentProperties } from "./gateway/PipelineDocument";

export type MappedCaseDocument = PresentationDocumentProperties & {
  presentationCategory: string;
  presentationFileName: string;
};
