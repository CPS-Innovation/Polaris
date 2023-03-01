import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

export const getRedactStatus = (
  id: string,
  pipelineState: CaseDetailsState["pipelineState"]
) => {
  if (pipelineState.haveData) {
    console.log("pipelineState>>>>>>", pipelineState.data);
    const redactStatus = pipelineState.data.documents.find(
      (document) => document.documentId === id
    )?.presentationStatuses?.redactStatus!;
    return redactStatus;
  }
  return null;
};
