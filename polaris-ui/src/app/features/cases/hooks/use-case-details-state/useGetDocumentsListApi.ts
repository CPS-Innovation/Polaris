import { useEffect } from "react";
import { useApi } from "../../../../common/hooks/useApi";
import { getDocumentsList } from "../../api/gateway-api";
import { DispatchType } from "../use-case-details-state/reducer";
import { RenameDocumentData } from "../../domain/gateway/RenameDocumentData";
import { ReclassifyDocumentData } from "../../domain/gateway/ReclassifyDocumentData";
import {
  handleReclassifyUpdateConfirmation,
  handleRenameUpdateConfirmation,
} from "../utils/refreshCycleDataUpdate";
export const useGetDocumentsListApi = (
  urn: string,
  caseId: number,
  startDocumentListRefresh: boolean,
  renameDocuments: RenameDocumentData[],
  reclassifyDocuments: ReclassifyDocumentData[],
  dispatch: DispatchType
) => {
  const documentsListState = useApi(
    getDocumentsList,
    [urn, caseId],
    startDocumentListRefresh
  );
  useEffect(() => {
    if (documentsListState.status !== "initial")
      dispatch({ type: "UPDATE_DOCUMENTS", payload: documentsListState });
  }, [documentsListState, dispatch]);

  useEffect(() => {
    if (documentsListState.status !== "succeeded") return;
    if (!documentsListState?.data) {
      return;
    }

    const activeRenameDoc = renameDocuments.find(
      (doc) => doc.saveRenameRefreshStatus === "updating"
    );

    if (activeRenameDoc) {
      const isUpdated = handleRenameUpdateConfirmation(
        documentsListState.data,
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
  }, [documentsListState, renameDocuments, dispatch]);

  useEffect(() => {
    if (documentsListState.status !== "succeeded") return;

    const activeReclassifyDoc = reclassifyDocuments.find(
      (doc) => doc.saveReclassifyRefreshStatus === "updating"
    );

    if (activeReclassifyDoc) {
      const isUpdated = handleReclassifyUpdateConfirmation(
        documentsListState.data,
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
  }, [documentsListState, reclassifyDocuments, dispatch]);
};
