import {
  PresentationDocumentProperties,
  ConversionStatus,
} from "./gateway/PipelineDocument";

export type LocalDocumentState = {
  [key: PresentationDocumentProperties["documentId"]]: {
    // [key: PresentationDocumentProperties["versionId"]]: {
    conversionStatus: ConversionStatus;
    // };
  };
};
