import { useEffect } from "react";
import { useApi } from "../../../../common/hooks/useApi";
import { getCaseDetails } from "../../api/gateway-api";
import { DispatchType } from "./reducer";
import { usePipelineApi } from "../use-pipeline-api/usePipelineApi";
import {
  handleReclassifyUpdateConfirmation,
  handleRenameUpdateConfirmation,
} from "../utils/refreshCycleDataUpdate";
import { CombinedState } from "../../domain/CombinedState";

export const useGetCaseData = (
  urn: string,
  caseId: number,
  combinedState: CombinedState,
  dispatch: DispatchType,
  isUnMounting: () => boolean
) => {
  // Load the case data
  const caseState = useApi(getCaseDetails, [urn, caseId]);
  useEffect(() => {
    if (caseState.status !== "initial")
      dispatch({ type: "UPDATE_CASE_DETAILS", payload: caseState });
  }, [caseState, dispatch]);

  // Load the accordion on first load and also if a startRefresh flag is passed
  const pipelineState = usePipelineApi(
    urn,
    caseId,
    combinedState.pipelineRefreshData,
    isUnMounting
  );
  useEffect(() => {
    dispatch({
      type: "UPDATE_PIPELINE",
      payload: pipelineState.pipelineResults,
    });
  }, [pipelineState.pipelineResults, dispatch]);

  useEffect(() => {
    if (!pipelineState.pipelineResults?.haveData) {
      return;
    }

    const activeRenameDoc = combinedState.renameDocuments.find(
      (doc) => doc.saveRenameRefreshStatus === "updating"
    );
    const activeReclassifyDoc = combinedState.reclassifyDocuments.find(
      (doc) => doc.saveReclassifyRefreshStatus === "updating"
    );
    if (!activeRenameDoc && !activeReclassifyDoc) return;
    if (activeRenameDoc) {
      const isUpdated = handleRenameUpdateConfirmation(
        pipelineState.pipelineResults.data,
        activeRenameDoc
      );
      if (isUpdated) {
        dispatch({
          type: "UPDATE_RENAME_DATA",
          payload: {
            properties: {
              documentId: activeRenameDoc.documentId,
              saveRenameRefreshStatus: "updated",
            },
          },
        });
      }
    }
    if (activeReclassifyDoc) {
      const isUpdated = handleReclassifyUpdateConfirmation(
        pipelineState.pipelineResults.data,
        activeReclassifyDoc
      );
      if (isUpdated) {
        dispatch({
          type: "UPDATE_RECLASSIFY_DATA",
          payload: {
            properties: {
              documentId: activeReclassifyDoc.documentId,
              saveReclassifyRefreshStatus: "updated",
            },
          },
        });
      }
    }
  }, [
    pipelineState.pipelineResults,
    combinedState.renameDocuments,
    combinedState.reclassifyDocuments,
    dispatch,
  ]);

  useEffect(() => {
    const { startRefresh } = combinedState.pipelineRefreshData;
    const caseStateStatus = combinedState.caseState.status;
    const pipelineResultStatus = pipelineState.pipelineResults.status;
    if (
      !startRefresh &&
      caseStateStatus === "succeeded" &&
      pipelineResultStatus === "initiating" &&
      !pipelineState.pipelineBusy
    ) {
      dispatch({
        type: "UPDATE_REFRESH_PIPELINE",
        payload: {
          startRefresh: true,
        },
      });
    }
    if (startRefresh) {
      dispatch({
        type: "UPDATE_REFRESH_PIPELINE",
        payload: {
          startRefresh: false,
        },
      });
    }
  }, [
    combinedState.pipelineRefreshData,
    combinedState.caseState.status,
    pipelineState.pipelineResults.status,
    pipelineState.pipelineBusy,
    dispatch,
  ]);
};
