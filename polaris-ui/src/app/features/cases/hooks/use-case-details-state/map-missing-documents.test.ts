import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PipelineResults } from "../../domain/PipelineResults";
import { mapMissingDocuments } from "./map-missing-documents";

const CASE_DOCUMENTS = [
  { documentId: "1", cmsOriginalFileName: "file-name-1" },
  { documentId: "2", cmsOriginalFileName: "file-name-2" },
] as MappedCaseDocument[];

describe("mapMissingDocuments", () => {
  it("can return missing documents when pipeline is complete", () => {
    const pipelineResults = {
      status: "Completed",
      transactionId: "",
      documents: [
        { documentId: "1", status: "OcrAndIndexFailure" },
        { documentId: "2", status: "Indexed" },
      ],
    } as PipelineResults;

    const result = mapMissingDocuments(pipelineResults, CASE_DOCUMENTS);

    expect(result).toEqual([
      {
        documentId: "1",
        fileName: "file-name-1",
      },
    ]);
  });

  it("can cope with a failed document in the pipeline that is not present in the documents array", () => {
    const pipelineResults = {
      status: "Completed",
      transactionId: "",
      documents: [
        { documentId: "3", status: "OcrAndIndexFailure" },
        { documentId: "1", status: "Indexed" },
      ],
    } as PipelineResults;

    const result = mapMissingDocuments(pipelineResults, CASE_DOCUMENTS);

    expect(result).toEqual([
      {
        documentId: "3",
        fileName: "",
      },
    ]);
  });
});
