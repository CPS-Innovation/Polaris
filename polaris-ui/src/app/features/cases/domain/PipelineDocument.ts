import { CmsDocCategory } from "./CmsDocCategory";
import { CmsDocType } from "./CmsDocType";

export type PipelineDocumentProperties = {
  documentId: string;
  pdfBlobName: string;
  status:
    | "None"
    | "PdfUploadedToBlob"
    | "Indexed"
    | "NotFoundInCDE"
    | "UnableToConvertToPdf"
    | "UnexpectedFailure"
    | "OcrAndIndexFailure";
};

export type PresentationStatuses = {
  viewStatus: "Ok" | "OnlyAvailableInCms" | "FailedConversion";
  redactStatus:
    | null
    | "Ok"
    | "DocTypeNotAllowed"
    | "OriginalFileTypeNotAllowed";
};

export type PresentationDocumentProperties = {
  documentId: string;
  CmsDocumentId?: string;
  cmsOriginalFileName: string;
  cmsMimeType: string;
  cmsFileCreatedDate: string;
  cmsDocCategory: CmsDocCategory;
  // documents in CMS are not guaranteed to have a cmsDocType
  cmsDocType: CmsDocType;
  presentationStatuses?: PresentationStatuses;
};

export type PipelineDocument = PipelineDocumentProperties &
  PresentationDocumentProperties;
