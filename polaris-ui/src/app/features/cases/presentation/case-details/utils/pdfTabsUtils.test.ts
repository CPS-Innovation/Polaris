import { AsyncPipelineResult } from "../../../hooks/use-pipeline-api/AsyncPipelineResult";
import { PipelineResults } from "../../../domain/PipelineResults";
import { getRedactStatus } from "./pdfTabsUtils";

describe("getRedactStatus util", () => {
  it("getRedactStatus should return null if pipelineState.haveData is false", () => {
    const pipelineState: AsyncPipelineResult<PipelineResults> = {
      status: "initiating",
      haveData: false,
    };
    const result = getRedactStatus("1", pipelineState);
    expect(result).toEqual(null);
  });
  it("getRedactStatus should return redact status of the document with matching documentId from pipelineSate documents", () => {
    const pipelineState: AsyncPipelineResult<PipelineResults> = {
      status: "complete",
      haveData: true,
      data: {
        transactionId: "abc",
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
            presentationStatuses: {
              viewStatus: "OnlyAvailableInCms",
              redactStatus: "Ok",
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
              viewStatus: "Ok",
              redactStatus: "DocTypeNotAllowed",
            },
          },
          {
            documentId: "3",
            cmsDocumentId: "3",
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
              viewStatus: "Ok",
              redactStatus: null,
            },
          },
        ],
      },
    };

    expect(getRedactStatus("1", pipelineState)).toEqual("Ok");
    expect(getRedactStatus("2", pipelineState)).toEqual("DocTypeNotAllowed");
    expect(getRedactStatus("3", pipelineState)).toEqual(null);
  });
  it("getRedactStatus should return null, if it couldn't find a matching documentId from pipelineSate documents", () => {
    const pipelineState: AsyncPipelineResult<PipelineResults> = {
      status: "complete",
      haveData: true,
      data: {
        transactionId: "abc",
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
            presentationStatuses: {
              viewStatus: "OnlyAvailableInCms",
              redactStatus: "Ok",
            },
          },
        ],
      },
    };

    expect(getRedactStatus("12", pipelineState)).toEqual(null);
  });
});
