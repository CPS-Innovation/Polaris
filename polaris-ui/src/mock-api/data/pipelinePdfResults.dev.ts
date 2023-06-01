import { PipelineResults } from "../../app/features/cases/domain/gateway/PipelineResults";
import { PipelinePdfResultsDataSource } from "./types/PipelinePdfResultsDataSource";

const dataSource: PipelinePdfResultsDataSource = () => getPipelinePdfResults(5);

export default dataSource;

const pipelinePdfResult: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  documentsRetrieved: new Date().toISOString(),
  documents: [
    {
      documentId: "1",
      cmsDocumentId: "1",
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
      presentationFileName: "MCLOVEMG3 very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
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
      presentationFileName: "CM01  very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 11,
        documentType: "MG11",
        documentCategory: "MGForm",
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
      presentationFileName: "MG05MCLOVE very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 5,
        documentType: "MG5",
        documentCategory: "MGForm",
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
      presentationFileName: "MG06_3June  very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "MG6",
        documentCategory: "MGForm",
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
      presentationFileName: "MG06_10june  very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
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
      presentationFileName: "MCLOVEMG3  very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
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
      presentationFileName: "CM01  very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: -1,
        documentType: "Other Comm (In)",
        documentCategory: "MGForm",
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
      presentationFileName: "MG05MCLOVE very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 5,
        documentType: "MG5",
        documentCategory: "MGForm",
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
      presentationFileName: "MG06_3June  very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-03",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "MG6",
        documentCategory: "MGForm",
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
      presentationFileName: "MG06_10june  very long",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-10",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "MG6",
        documentCategory: "MGForm",
      },

      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
    },
  ],
};

const getPipelinePdfResults = (resultsCount: number) => {
  let resultsArray = Array(resultsCount)
    .fill({})
    .map((value, index) => ({
      ...pipelinePdfResult,

      processingCompleted: new Date(
        new Date().getTime() + index * 1000
      ).toISOString(),
      documentsRetrieved: new Date(
        new Date().getTime() + index * 1000
      ).toISOString(),
      documents: pipelinePdfResult.documents.map((document) => ({
        ...document,
        polarisDocumentVersionId: document.polarisDocumentVersionId + index,
      })),
    }));

  return resultsArray;
};
