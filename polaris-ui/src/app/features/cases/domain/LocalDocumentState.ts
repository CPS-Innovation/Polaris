import {
  PresentationDocumentProperties,
  ConversionStatus,
  GroupedConversionStatus,
} from "./gateway/PipelineDocument";

export type LocalDocumentState = {
  [key: PresentationDocumentProperties["documentId"]]: {
    conversionStatus: ConversionStatus | GroupedConversionStatus;
  };
};
