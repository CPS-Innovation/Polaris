import { PipelineResults } from "../../app/features/cases/domain/gateway/PipelineResults";
import { PipelinePdfResultsDataSource } from "./types/PipelinePdfResultsDataSource";

const dataSource: PipelinePdfResultsDataSource = () => getPipelinePdfResults(5);

export default dataSource;

const pipelinePdfResult: PipelineResults = {
  status: "Completed",
  processingCompleted: new Date().toISOString(),
  //documentsRetrieved: new Date().toISOString(),
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
      status: "UnexpectedFailure",
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
      documentId: "7",
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
      documentId: "10",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "11",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "12",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
    },
    {
      documentId: "13",
      status: "Indexed",
      conversionStatus: "DocumentConverted",
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
    }));
  return resultsArray;
};
