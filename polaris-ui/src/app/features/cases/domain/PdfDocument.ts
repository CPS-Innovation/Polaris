import { CmsDocCategory } from "./CmsDocCategory";
import { CmsDocType } from "./CmsDocType";

export type PipeLineDocumentProperties = {
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

export type PresentationDocumentProperties = {
  documentId: string;
  fileName: string;
  createdDate: string;
  cmsDocCategory: CmsDocCategory;
  // documents in CMS are not guaranteed to have a cmsDocType
  cmsDocType: CmsDocType;
};

export type PdfDocument = PipeLineDocumentProperties &
  PresentationDocumentProperties;
