export type PdfDocument = {
  documentId: number;
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
