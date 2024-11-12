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

  // Load the accordion on first load and also if a startDocumentRefresh flag is passed
  const pipelineState = usePipelineApi(
    urn,
    caseId,
    combinedState.pipelineRefreshData,
    isUnMounting
  );

  // When pipeline results have changed, update our state
  useEffect(() => {
    dispatch({
      type: "UPDATE_PIPELINE",
      payload: pipelineState.pipelineResults,
    });
  }, [pipelineState.pipelineResults, dispatch]);

  // On a pipeline update, deal with renamed docs
  // useEffect(() => {
  //   if (!pipelineState.pipelineResults?.haveData) {
  //     return;
  //   }

  //   const activeRenameDoc = combinedState.renameDocuments.find(
  //     (doc) => doc.saveRenameRefreshStatus === "updating"
  //   );

  //   if (activeRenameDoc) {
  //     const isUpdated = handleRenameUpdateConfirmation(
  //       documentsState,
  //       activeRenameDoc
  //     );
  //     if (isUpdated) {
  //       dispatch({
  //         type: "UPDATE_RENAME_DATA",
  //         payload: {
  //           properties: {
  //             documentId: activeRenameDoc.documentId,
  //             saveRenameRefreshStatus: "updated",
  //           },
  //         },
  //       });
  //     }
  //   }
  // }, [pipelineState.pipelineResults, combinedState.renameDocuments, dispatch]);

  // On a pipeline update, deal with reclassified docs
  // useEffect(() => {
  //   if (!pipelineState.pipelineResults?.haveData) {
  //     return;
  //   }

  //   const activeReclassifyDoc = combinedState.reclassifyDocuments.find(
  //     (doc) => doc.saveReclassifyRefreshStatus === "updating"
  //   );

  //   if (activeReclassifyDoc) {
  //     const isUpdated = handleReclassifyUpdateConfirmation(
  //       pipelineState.pipelineResults.data,
  //       activeReclassifyDoc
  //     );
  //     if (isUpdated) {
  //       dispatch({
  //         type: "UPDATE_RECLASSIFY_DATA",
  //         payload: {
  //           properties: {
  //             documentId: activeReclassifyDoc.documentId,
  //             saveReclassifyRefreshStatus: "updated",
  //           },
  //         },
  //       });
  //     }
  //   }
  // }, [
  //   pipelineState.pipelineResults,
  //   combinedState.reclassifyDocuments,
  //   dispatch,
  // ]);

  // This triggers the first ever load of the pipeline
  useEffect(() => {
    const { startDocumentRefresh } = combinedState.documentRefreshData;
    // if (
    //   // if we have not started refresh...
    //   !startDocumentRefresh &&
    //   // ... and only if the case data has loaded ...
    //   combinedState.caseState.status === "succeeded" &&
    //   // ... the pipelineResults are not in an in-flight state...
    //   combinedState.documentsState.status === "loading"
    // ) {
    //   // ... then lets start a refresh
    //   dispatch({
    //     type: "UPDATE_REFRESH_PIPELINE",
    //     payload: {
    //       startDocumentRefresh: true,
    //     },
    //   });
    // }

    // if (
    //   // if we have not started refresh...
    //   !startDocumentRefresh &&
    //   // ... and only if the case data has loaded ...
    //   combinedState.caseState.status === "succeeded" &&
    //   // ... the pipelineResults are not in an in-flight state...
    //   pipelineState.pipelineResults.status === "initiating" &&
    //   // ... and the pipeline has already been triggered ...
    //   !pipelineState.pipelineBusy
    // ) {
    //   // ... then lets start a refresh
    //   dispatch({
    //     type: "UPDATE_REFRESH_PIPELINE",
    //     payload: {
    //       startDocumentRefresh: true,
    //     },
    //   });
    // }

    if (startDocumentRefresh) {
      // Once startDocumentRefresh has been picked up by the reducer then we we will end up here
      //... and then we switch it off (I think)
      dispatch({
        type: "UPDATE_REFRESH_PIPELINE",
        payload: {
          startDocumentRefresh: false,
        },
      });
    }
  }, [
    combinedState.documentRefreshData,
    combinedState.caseState.status,
    pipelineState.pipelineResults.status,
    pipelineState.pipelineBusy,
    dispatch,
  ]);
};
