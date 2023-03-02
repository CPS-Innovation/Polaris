import { PipelineResults } from "../../app/features/cases/domain/PipelineResults";
import { PipelinePdfResultsDataSource } from "./types/PipelinePdfResultsDataSource";

const dataSource: PipelinePdfResultsDataSource = () => pipelinePdfResults;

export default dataSource;

const pipelinePdfResults: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  documents: [
    {
      documentId: "1",
      cmsDocumentId: "1",
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
      cmsOriginalFileName: "MCLOVEMG3",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-01",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 1,
        code: "MG11",
        name: "MG11 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "2",
      cmsDocumentId: "1",
      pdfBlobName: "CM01",
      status: "Indexed",
      cmsOriginalFileName: "CM01",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 2,
        code: "MG12",
        name: "MG12 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "3",
      cmsDocumentId: "3",
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
      cmsOriginalFileName: "MG05MCLOVE",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 3,
        code: "MG13",
        name: "MG13 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "Indexed",
      cmsOriginalFileName: "MG06_3June",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-04",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 4,
        code: "MG14",
        name: "MG14 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "Indexed",
      cmsOriginalFileName: "MG06_10june",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 5,
        code: "MG15",
        name: "MG15 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
  ],
};

export const missingDocsPipelinePdfResults: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  documents: [
    {
      documentId: "1",
      cmsDocumentId: "1",
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
      cmsOriginalFileName: "MCLOVEMG3  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 3,
        code: "MG3",
        name: "MG3 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "2",
      cmsDocumentId: "2",
      pdfBlobName: "CM01",
      status: "Indexed",
      cmsOriginalFileName: "CM01  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 11,
        code: "MG11",
        name: "MG11 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "3",
      cmsDocumentId: "3",
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
      cmsOriginalFileName: "MG05MCLOVE very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 5,
        code: "MG5",
        name: "MG5 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 6,
        code: "MG6",
        name: "MG6 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "UnableToConvertToPdf",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 3,
        code: "MG3",
        name: "MG3 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
  ],
};

export const allMissingDocsPipelinePdfResults: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  documents: [
    {
      documentId: "1",
      cmsDocumentId: "1",
      pdfBlobName: "MCLOVEMG3",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MCLOVEMG3  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 3,
        code: "MG3",
        name: "MG3 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "2",
      cmsDocumentId: "2",
      pdfBlobName: "CM01",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "CM01  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 11,
        code: "MG11",
        name: "MG11 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "3",
      cmsDocumentId: "3",
      pdfBlobName: "MG05MCLOVE",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MG05MCLOVE very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 5,
        code: "MG5",
        name: "MG5 File",
      },
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "OcrAndIndexFailure",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 6,
        code: "MG6",
        name: "MG6 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "UnableToConvertToPdf",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 3,
        code: "MG3",
        name: "MG3 File",
      },
      presentationStatuses: {
        viewStatus: "Ok", //| "OnlyAvailableInCms" | "FailedConversion"
        redactStatus: "Ok", // null | "DocTypeNotAllowed"| "OriginalFileTypeNotAllowed";
      },
    },
  ],
};
