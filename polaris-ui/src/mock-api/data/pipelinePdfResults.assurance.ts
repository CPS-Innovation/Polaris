import { PipelineResults } from "../../app/features/cases/domain/gateway/PipelineResults";
import { PipelinePdfResultsDataSource } from "./types/PipelinePdfResultsDataSource";
//the result count is set to 8 based on the maximum number of call tracker api call in a test suit, increase it when needed.
const dataSource: PipelinePdfResultsDataSource = () => getPipelinePdfResults(8);

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
      pdfBlobName: "specimen",
      status: "Indexed",
      cmsOriginalFileName: "specimen",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-01",
      cmsDocCategory: "MGForm",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        id: 1,
        code: "MG11",
        name: "MG11 File",
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
      cmsOriginalFileName: "CM01",
      cmsMimeType: "application/pdf",
      cmsFileCreatedDate: "2020-06-02",
      cmsDocCategory: "MGForm",
      polarisDocumentVersionId: 1,
      cmsDocType: {
        id: 2,
        code: "MG12",
        name: "MG12 File",
      },
      presentationFlags: {
        read: "Ok",
        write: "DocTypeNotAllowed",
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
      polarisDocumentVersionId: 1,
      cmsDocType: {
        id: 3,
        code: "MG13",
        name: "MG13 File",
      },
      presentationFlags: {
        read: "OnlyAvailableInCms",
        write: "Ok",
      },
    },
  ],
};
// this will return updated tracker data with updated polarisDocumentVersionId, processingCompleted and documentsRetrieved needed for te redaction refresh flow
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
