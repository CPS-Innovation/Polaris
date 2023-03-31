import { CmsDocCategory } from "./CmsDocCategory";
import { CmsDocType } from "./CmsDocType";

export type PipelineDocumentProperties = {
  documentId: string;
  pdfBlobName: string;
  isPdfAvailable?: boolean;
  status:
    | "None"
    | "PdfUploadedToBlob"
    | "Indexed"
    | "NotFoundInCDE"
    | "UnableToConvertToPdf"
    | "UnexpectedFailure"
    | "OcrAndIndexFailure";
};
export type PresentationFlags = {
  read: "Ok" | "OnlyAvailableInCms";
  write:
    | "Ok"
    | "OnlyAvailableInCms"
    | "DocTypeNotAllowed"
    | "OriginalFileTypeNotAllowed";
};

export type PresentationDocumentProperties = {
  documentId: string;
  cmsDocumentId: string;
  cmsOriginalFileName: string;
  polarisDocumentVersionId: number;
  cmsMimeType: string;
  cmsFileCreatedDate: string;
  cmsDocCategory: CmsDocCategory;
  // documents in CMS are not guaranteed to have a cmsDocType
  cmsDocType: CmsDocType;
  presentationFlags: PresentationFlags;
  cmsVersionId: number;
};

export type PipelineDocument = PipelineDocumentProperties &
  PresentationDocumentProperties;
