import { AsyncPipelineResult } from "../../../hooks/use-pipeline-api/AsyncPipelineResult";
import { PipelineResults } from "../../../domain/PipelineResults";
export const getRedactStatus = (
  id: string,
  pipelineState: AsyncPipelineResult<PipelineResults>
) => {
  var status =
    pipelineState.haveData &&
    pipelineState.data.documents.find((document) => document.documentId === id)
      ?.presentationFlags.write;

  if (!status) {
    throw new Error(
      "Unable to resolve `presentationFlags.write` for a document"
    );
  }

  return status;
};
