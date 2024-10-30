import { CmsDocType } from "./CmsDocType";

export type PipelineDocumentProperties = {
  documentId: string;
  conversionStatus: ConversionStatus;
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

export type Classification =
  | null
  | "Statement"
  | "Exhibit"
  | "Other"
  | "DefenceStatement";

export type PresentationDocumentProperties = {
  documentId: string;
  cmsOriginalFileName: string;
  presentationTitle: string;
  versionId: number;
  cmsFileCreatedDate: string;
  categoryListOrder: number | null;
  // documents in CMS are not guaranteed to have a cmsDocType
  cmsDocType: CmsDocType;
  presentationFlags: PresentationFlags;
  parentDocumentId: string | null;
  witnessId: number | null;
  hasFailedAttachments: boolean;
  hasNotes: boolean;
  conversionStatus: ConversionStatus;
  isOcrProcessed: boolean;
  isUnused: boolean;
  isInbox: boolean;
  classification: Classification;
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
