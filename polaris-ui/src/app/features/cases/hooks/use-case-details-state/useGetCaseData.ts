import { useEffect } from "react";
import { useApi } from "../../../../common/hooks/useApi";
import { getCaseDetails } from "../../api/gateway-api";
import { DispatchType } from "./reducer";
import { usePipelineApi } from "../use-pipeline-api/usePipelineApi";
import { CombinedState } from "../../domain/CombinedState";
import { useGetDocumentsListApi } from "./useGetDocumentsListApi";

export const useGetCaseData = (
  urn: string,
  caseId: number,
  combinedState: CombinedState,
  dispatch: DispatchType,
  isUnMounting: () => boolean
) => {
  // Load case data
  const caseState = useApi(getCaseDetails, [urn, caseId]);
  useEffect(() => {
    if (caseState.status !== "initial")
      dispatch({ type: "UPDATE_CASE_DETAILS", payload: caseState });
  }, [caseState, dispatch]);

  useGetDocumentsListApi(
    urn,
    caseId,
    combinedState.documentRefreshData?.startDocumentRefresh,
    combinedState.renameDocuments,
    combinedState.reclassifyDocuments,
    dispatch
  );

  // trigger the pipeline refresh when the  startPipelineRefresh is true
  usePipelineApi(
    urn,
    caseId,
    combinedState.pipelineRefreshData,
    combinedState.notificationState.lastModifiedDateTime,
    isUnMounting,
    dispatch
  );

  // This triggers the first ever load of the pipeline
  useEffect(() => {
    const { startDocumentRefresh } = combinedState.documentRefreshData;
    const { startPipelineRefresh } = combinedState.pipelineRefreshData;

    // Once startPipelineRefresh has been picked up by the reducer then we we will end up here and then we switch it off
    if (startPipelineRefresh) {
      dispatch({
        type: "UPDATE_PIPELINE_REFRESH",
        payload: {
          startPipelineRefresh: false,
        },
      });
    }

    if (startDocumentRefresh) {
      // Once startDocumentRefresh has been picked up by the reducer then we we will end up here and then we switch it off
      dispatch({
        type: "UPDATE_DOCUMENT_REFRESH",
        payload: {
          startDocumentRefresh: false,
        },
      });
    }
  }, [
    combinedState.documentRefreshData,
    combinedState.pipelineRefreshData,
    dispatch,
  ]);
};
