import { TagType } from "./TagType";
import { WitnessIndicator } from "./WitnessIndicators";
import { PresentationDocumentProperties } from "./gateway/PipelineDocument";

export type MappedCaseDocument = PresentationDocumentProperties & {
  presentationCategory: string;
  presentationSubCategory: string | null;
  attachments: { documentId: string; name: string }[];
  witnessIndicators: WitnessIndicator[];
  tags: TagType[];
};
