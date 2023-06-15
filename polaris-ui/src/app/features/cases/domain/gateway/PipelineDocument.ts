import { CmsDocType } from "./CmsDocType";

export type PipelineDocumentProperties = {
  polarisDocumentId?: string;
  documentId: string;
  pdfBlobName: string;
  isPdfAvailable?: boolean;
  status:
    | "New"
    | "PdfUploadedToBlob"
    | "Indexed"
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
  presentationTitle: string;
  polarisDocumentVersionId: number;
  cmsMimeType: string;
  cmsFileCreatedDate: string;
  categoryListOrder?: number | null;
  // documents in CMS are not guaranteed to have a cmsDocType
  cmsDocType: CmsDocType;
  presentationFlags: PresentationFlags;
};

export type PipelineDocument = PipelineDocumentProperties &
  PresentationDocumentProperties;
