import { AsyncPipelineResult } from "../../../hooks/use-pipeline-api/AsyncPipelineResult";
import { PipelineResults } from "../../../domain/PipelineResults";
export const getRedactStatus = (
  id: string,
  pipelineState: AsyncPipelineResult<PipelineResults>
) => {
  if (pipelineState.haveData) {
    const redactStatus = pipelineState.data.documents.find(
      (document) => document.documentId === id
    )?.presentationStatuses?.redactStatus;
    if (redactStatus) {
      return redactStatus;
    }
    return null;
  }
  return null;
};
