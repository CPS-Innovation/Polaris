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
    | "OriginalFileTypeNotAllowed"
    | "IsDispatched"
    | "IsRedactionServiceOffline";
};

export type ConversionStatus =
  | "DocumentConverted"
  | "PdfEncrypted"
  | "DocumentTypeUnsupported"
  | "AsposePdfPasswordProtected"
  | "AsposePdfInvalidFileFormat"
  | "AsposePdfException"
  | "AsposeWordsUnsupportedFileFormat"
  | "AsposeWordsPasswordProtected"
  | "AsposeSlidesPasswordProtected"
  | "AsposeCellsGeneralError"
  | "AsposeImagingCannotLoad"
  | "UnexpectedError";

export const mapConversionStatusToMessage = (
  status: ConversionStatus
): string => {
  switch (status) {
    case "DocumentConverted":
      return "Document converted";
    case "PdfEncrypted":
    case "AsposePdfPasswordProtected":
    case "AsposeWordsPasswordProtected":
    case "AsposeSlidesPasswordProtected":
      return "file is password protected";
    case "DocumentTypeUnsupported":
    case "AsposeWordsUnsupportedFileFormat":
    case "AsposePdfInvalidFileFormat":
      return "document type unsupported";
    case "AsposeCellsGeneralError":
    case "AsposeImagingCannotLoad":
    case "UnexpectedError":
    case "AsposePdfException":
      return "";
  }
};
export type PresentationDocumentProperties = {
  documentId: string;
  cmsDocumentId: string;
  cmsOriginalFileName: string;
  presentationTitle: string;
  polarisDocumentVersionId: number;
  cmsOriginalFileExtension: string | null;
  cmsFileCreatedDate: string;
  categoryListOrder: number | null;
  // documents in CMS are not guaranteed to have a cmsDocType
  cmsDocType: CmsDocType;
  presentationFlags: PresentationFlags;
  polarisParentDocumentId: string | null;
  witnessId: number | null;
  hasFailedAttachments: boolean;
  hasNotes: boolean;
  conversionStatus: ConversionStatus;
  isUnused: boolean;
  isInbox: boolean;
  classification:
    | string
    | null
    | "Statement"
    | "Exhibit"
    | "Other"
    | "DefenceStatement";
  isWitnessManagement: boolean;
  canReclassify: boolean;
  canRename: boolean;
  renameStatus:
    | "CanRename"
    | "IsWitnessManagement"
    | "IsDispatched"
    | "IsStatement"
    | "IsDefenceStatement";
  reference: string | null;
};

export type PipelineDocument = PipelineDocumentProperties &
  PresentationDocumentProperties;
