import { Reducer } from "react";
import { AsyncActionHandlers } from "use-reducer-async";
import {
  cancelCheckoutDocument,
  checkoutDocument,
  getPdfSasUrl,
  saveRedactions,
} from "../../api/gateway-api";
import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { mapRedactionSaveRequest } from "./map-redaction-save-request";
import { reducer } from "./reducer";
import * as HEADERS from "../../api/header-factory";
import { ApiError } from "../../../../common/errors/ApiError";

const LOCKED_STATES_REQUIRING_UNLOCK: CaseDocumentViewModel["clientLockedState"][] =
  ["locked", "locking"];

const UNLOCKED_STATES_REQUIRING_LOCK: CaseDocumentViewModel["clientLockedState"][] =
  ["unlocked", "unlocking", "locked-by-other-user"];

type State = Parameters<typeof reducer>[0];
type Action = Parameters<typeof reducer>[1];

type AsyncActions =
  | {
      type: "ADD_REDACTION_AND_POTENTIALLY_LOCK";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
        redaction: NewPdfHighlight;
      };
    }
  | {
      type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
        redactionId: string;
      };
    }
  | {
      type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
      };
    }
  | {
      type: "SAVE_REDACTIONS";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
      };
    }
  | {
      type: "REQUEST_OPEN_PDF";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
        mode: CaseDocumentViewModel["mode"];
      };
    }
  | {
      type: "REQUEST_OPEN_PDF_IN_NEW_TAB";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
      };
    }
  | {
      type: "UNLOCK_DOCUMENTS";
      payload: {
        documentIds: CaseDocumentViewModel["documentId"][];
      };
    };

export const reducerAsyncActionHandlers: AsyncActionHandlers<
  Reducer<State, Action>,
  AsyncActions
> = {
  REQUEST_OPEN_PDF_IN_NEW_TAB:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId },
      } = action;

      const { urn, caseId } = getState();

      const sasUrl = await getPdfSasUrl(urn, caseId, documentId);

      dispatch({
        type: "OPEN_PDF_IN_NEW_TAB",
        payload: { documentId, sasUrl },
      });
    },
  REQUEST_OPEN_PDF:
    ({ dispatch }) =>
    async (action) => {
      const { payload } = action;

      const headers = {
        ...HEADERS.correlationId(),
        ...(await HEADERS.auth()),
      };

      dispatch({
        type: "OPEN_PDF",
        payload: { ...payload, headers },
      });
    },

  ADD_REDACTION_AND_POTENTIALLY_LOCK:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;

      const { documentId } = payload;
      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const { clientLockedState } = items.find(
        (item) => item.documentId === documentId
      )!;

      const documentRequiresLocking =
        UNLOCKED_STATES_REQUIRING_LOCK.includes(clientLockedState);

      if (!documentRequiresLocking) {
        dispatch({ type: "ADD_REDACTION", payload });
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { documentId, lockedState: "locking" },
      });
      try {
        await checkoutDocument(urn, caseId, documentId);

        dispatch({ type: "ADD_REDACTION", payload });
        dispatch({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: {
            documentId,
            lockedState: "locked",
          },
        });
      } catch (error: unknown) {
        const { code } = error as ApiError;
        /* NOTE: Ideally we a need another api request to get the locked status of a document , 
        which is fired when a document is opened in a Tab, 
        So that based on that we can update the user the the document is locked by another user if it is not available and 
        block the selection and subsequent redaction on locked files"
        */
        if (code === 409) {
          dispatch({
            type: "UPDATE_DOCUMENT_LOCK_STATE",
            payload: {
              documentId,
              lockedState: "locked-by-other-user",
            },
          });
          dispatch({
            type: "SHOW_ERROR_MODAL",
            payload: {
              title: "Failed to redact document",
              message:
                "It is not possible to redact as the document is already checked out by another user. Please try again later.",
            },
          });
          return;
        }
        dispatch({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: {
            documentId,
            lockedState: "unlocked",
          },
        });
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            title: "Something went wrong!",
            message: "Failed to checkout document. Please try again later.",
          },
        });
      }
    },

  REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;

      const { documentId } = payload;
      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;

      const { redactionHighlights, clientLockedState: lockedState } = document;

      dispatch({ type: "REMOVE_REDACTION", payload });

      const requiresCheckIn =
        // this is the last existing highlight
        redactionHighlights.length === 1 &&
        LOCKED_STATES_REQUIRING_UNLOCK.includes(lockedState);

      if (!requiresCheckIn) {
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { documentId, lockedState: "unlocking" },
      });

      await cancelCheckoutDocument(urn, caseId, documentId);

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: {
          documentId,
          lockedState: "unlocked",
        },
      });
    },

  REMOVE_ALL_REDACTIONS_AND_UNLOCK:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;

      const { documentId } = payload;
      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;

      const { clientLockedState: lockedState } = document;

      const requiresCheckIn =
        LOCKED_STATES_REQUIRING_UNLOCK.includes(lockedState);

      dispatch({ type: "REMOVE_ALL_REDACTIONS", payload });

      if (!requiresCheckIn) {
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { documentId, lockedState: "unlocking" },
      });

      await cancelCheckoutDocument(urn, caseId, documentId);

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: {
          documentId,
          lockedState: "unlocked",
        },
      });
    },

  SAVE_REDACTIONS:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;
      const { documentId } = payload;

      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;

      const { redactionHighlights, polarisDocumentVersionId } = document;

      const redactionSaveRequest = mapRedactionSaveRequest(
        documentId,
        redactionHighlights
      );
      try {
        await saveRedactions(urn, caseId, documentId, redactionSaveRequest);

        dispatch({
          type: "REMOVE_ALL_REDACTIONS",
          payload: { documentId },
        });

        dispatch({
          type: "UPDATE_REFRESH_PIPELINE",
          payload: {
            startRefresh: true,
            savedDocumentDetails: {
              documentId: documentId,
              polarisDocumentVersionId: polarisDocumentVersionId,
            },
          },
        });
      } catch (e) {
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            title: "Something went wrong!",
            message: "Failed to save redaction. Please try again later.",
          },
        });
      }

      // todo: does a save IN THE CGI API check a document in automatically?
      //await cancelCheckoutDocument(urn, caseId, documentId);
    },

  UNLOCK_DOCUMENTS:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentIds },
      } = action;

      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const requests = documentIds.map((documentId) =>
        cancelCheckoutDocument(urn, caseId, documentId)
      );

      Promise.allSettled(requests);
    },
};
