import { AsyncPipelineResult } from "../../../hooks/use-pipeline-api/AsyncPipelineResult";
import { PipelineResults } from "../../../domain/PipelineResults";
export const getRedactStatus = (
  id: string,
  pipelineState: AsyncPipelineResult<PipelineResults>
) => {
  console.log("id>>>>", id);
  var status =
    pipelineState.haveData &&
    pipelineState.data.documents.find(
      (document) => document.cmsDocumentId === id
    )?.presentationFlags?.write;
  if (!status) {
    throw new Error(
      "Unable to resolve `presentationFlags.write` for a document"
    );
  }

  return status;
};
