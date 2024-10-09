import { PipelineResults } from "../../../app/features/cases/domain/gateway/PipelineResults";

export type PipelineResultsWithPdfBlobNames = PipelineResults & {
  documents: { pdfBlobName: string }[];
};

export type PipelinePdfResultsDataSource =
  () => PipelineResultsWithPdfBlobNames[];

// For the purposes of this mocking approach, lets add a pdfBlobName field in to the documents
//  array to let msw know which file to use to when the client asks for the binary of a pdf.
export type DocumentWithPdfBlobName =
  PipelineResultsWithPdfBlobNames["documents"][0];

// But when the documents json is returned lets make sure that the pdfBlobName is cleaned away
export const removePdfBlobName = (
  docs: PipelineResultsWithPdfBlobNames
): PipelineResults => ({
  ...docs,
  documents: (docs.documents as DocumentWithPdfBlobName[]).map((doc) => {
    const { pdfBlobName, ...rest } = doc;
    return { ...rest };
  }),
});
