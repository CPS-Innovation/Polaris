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
      cmsOriginalFileName: "MCLOVEMG3 very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 3,
        code: "MG3",
        name: "MG3 File",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
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
      presentationFlags: {
        read: "Ok",
        write: "Ok",
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
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "Indexed",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 6,
        code: "MG6",
        name: "MG6 File",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "Indexed",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 3,
        code: "MG3",
        name: "MG3 File",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "6",
      cmsDocumentId: "6",
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
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "7",
      cmsDocumentId: "7",
      pdfBlobName: "CM01",
      status: "Indexed",
      cmsOriginalFileName: "CM01  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: -1,
        code: "Other Comm (In)",
        name: "Other Comm (In) File",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "8",
      cmsDocumentId: "8",
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
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "9",
      cmsDocumentId: "9",
      pdfBlobName: "MG06_3June",
      status: "Indexed",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 6,
        code: "MG6",
        name: "MG6 File",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
    {
      documentId: "10",
      cmsDocumentId: "10",
      pdfBlobName: "MG06_10june",
      status: "Indexed",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      cmsDocCategory: "MGForm",
      cmsDocType: {
        id: 6,
        code: "MG6",
        name: "MG6 File",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
  ],
};
