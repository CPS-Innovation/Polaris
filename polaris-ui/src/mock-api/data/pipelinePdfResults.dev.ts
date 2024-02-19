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
      cmsOriginalFileName: "MCLOVEMG3 very long .docx",
      presentationTitle: "UM MCLOVEMG3 very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2022-06-02T21:22:33Z",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 1029,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: "4",
      witnessId: null,
      hasFailedAttachments: true,
    },
    {
      documentId: "2",
      cmsDocumentId: "2",
      pdfBlobName: "CM01",
      status: "Indexed",
      cmsOriginalFileName: "CM01  very long .docx",
      presentationTitle: "CM01 Item 4 very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-02T11:45:33Z",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 1029,
        documentType: "MG11",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: "4",
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "3",
      cmsDocumentId: "3",
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
      cmsOriginalFileName: "MG05MCLOVE very long .docx",
      presentationTitle: "MG05MCLOVE very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 5,
        documentType: "MG5",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: "4",
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "4",
      cmsDocumentId: "4",
      pdfBlobName: "MG06_3June",
      status: "Indexed",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      presentationTitle: "MG06_3June  very long",
      cmsOriginalFileExtension: ".hte",
      cmsFileCreatedDate: "2020-06-03",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "MG6",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: null,
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "5",
      cmsDocumentId: "5",
      pdfBlobName: "MG06_10june",
      status: "Indexed",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      presentationTitle: "MG06_10june  very long",
      cmsOriginalFileExtension: ".hte",
      cmsFileCreatedDate: "2020-06-10",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: null,
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "6",
      cmsDocumentId: "6",
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
      cmsOriginalFileName: "MCLOVEMG3  very long .docx",
      presentationTitle: "MCLOVEMG3  very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: "4",
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "7",
      cmsDocumentId: "7",
      pdfBlobName: "CM01",
      status: "Indexed",
      cmsOriginalFileName: "CM01  very long .docx",
      presentationTitle: "CM01  very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: -1,
        documentType: "Other Comm (In)",
        documentCategory: "MGForm",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: null,
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "8",
      cmsDocumentId: "8",
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
      cmsOriginalFileName: "MG05MCLOVE very long .docx",
      presentationTitle: "MG05MCLOVE very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 5,
        documentType: "MG5",
        documentCategory: "MGForm",
      },

      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: null,
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "9",
      cmsDocumentId: "9",
      pdfBlobName: "MG06_3June",
      status: "Indexed",
      cmsOriginalFileName: "MG06_3June  very long .docx",
      presentationTitle: "MG06_3June  very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-03",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "MG6",
        documentCategory: "MGForm",
      },

      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: null,
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "10",
      cmsDocumentId: "10",
      pdfBlobName: "MG06_10june",
      status: "Indexed",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      presentationTitle: "MG06_10june  very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-10",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 6,
        documentType: "DAC",
        documentCategory: "MGForm",
      },

      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: null,
      witnessId: null,
      hasFailedAttachments: false,
    },
    {
      documentId: "10",
      cmsDocumentId: "10",
      pdfBlobName: "MG06_10june",
      status: "Indexed",
      cmsOriginalFileName: "MG06_10june  very long .docx",
      presentationTitle: "MG06_10june  very long",
      cmsOriginalFileExtension: ".pdf",
      cmsFileCreatedDate: "2020-06-02",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
      cmsDocType: {
        documentTypeId: 1031,
        documentType: "Statement",
        documentCategory: "UsedStatement",
      },
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      polarisParentDocumentId: null,
      witnessId: 2762766,
      hasFailedAttachments: false,
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
