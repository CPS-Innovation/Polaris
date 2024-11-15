import { PipelineResults } from "../../app/features/cases/domain/gateway/PipelineResults";
import { PipelinePdfResultsDataSource } from "./types/PipelinePdfResultsDataSource";
//the result count is set to 9 based on the maximum number of call tracker api call in a test suit, increase it when needed.

const dataSource: PipelinePdfResultsDataSource = () => getPipelinePdfResults(9);

export default dataSource;

const pipelinePdfResult: PipelineResults = {
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  documentsRetrieved: new Date().toISOString(),
  documents: [
    {
      documentId: "1",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "2",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "3",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "4",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "5",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "6",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "8",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "9",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "10",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "12",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
  ],
};

// this will return updated tracker data with updated versionId, processingCompleted and documentsRetrieved needed for te redaction refresh flow
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
    }));
  return resultsArray;
};

export const missingDocsPipelinePdfResults: PipelineResults = {
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  documentsRetrieved: new Date().toISOString(),
  documents: [
    {
      documentId: "1",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "3",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "4",
      status: "OcrAndIndexFailure",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "5",
      status: "UnableToConvertToPdf",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "12",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
  ],
};

export const allMissingDocsPipelinePdfResults: PipelineResults = {
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  documentsRetrieved: new Date().toISOString(),
  documents: [
    {
      documentId: "1",
      status: "OcrAndIndexFailure",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "2",
      status: "OcrAndIndexFailure",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "3",
      status: "OcrAndIndexFailure",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "4",
      status: "OcrAndIndexFailure",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "5",
      status: "UnableToConvertToPdf",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "12",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
  ],
};
