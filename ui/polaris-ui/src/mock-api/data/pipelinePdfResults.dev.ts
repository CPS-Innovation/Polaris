import { PipelineResults } from "../../app/features/cases/domain/PipelineResults";
import { PipelinePdfResultsDataSource } from "./types/PipelinePdfResultsDataSource";

const dataSource: PipelinePdfResultsDataSource = () => pipelinePdfResults;

export default dataSource;

const pipelinePdfResults: PipelineResults = {
  transactionId: "121",
  status: "Completed",
  documents: [
    {
      documentId: 1,
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
    },
    {
      documentId: 2,
      pdfBlobName: "CM01",
      status: "Indexed",
    },
    {
      documentId: 3,
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
    },
    {
      documentId: 4,
      pdfBlobName: "MG06_3June",
      status: "Indexed",
    },
    {
      documentId: 5,
      pdfBlobName: "MG06_10june",
      status: "Indexed",
    },
    {
      documentId: 6,
      pdfBlobName: "MCLOVEMG3",
      status: "Indexed",
    },
    {
      documentId: 7,
      pdfBlobName: "CM01",
      status: "Indexed",
    },
    {
      documentId: 8,
      pdfBlobName: "MG05MCLOVE",
      status: "Indexed",
    },
    {
      documentId: 9,
      pdfBlobName: "MG06_3June",
      status: "Indexed",
    },
    {
      documentId: 10,
      pdfBlobName: "MG06_10june",
      status: "Indexed",
    },
  ],
};
