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

const LOCKED_STATES_REQUIRING_UNLOCK: CaseDocumentViewModel["clientLockedState"][] =
  ["locked", "locking"];

const UNLOCKED_STATES_REQUIRING_LOCK: CaseDocumentViewModel["clientLockedState"][] =
  ["unlocked", "unlocking"];

type State = Parameters<typeof reducer>[0];
type Action = Parameters<typeof reducer>[1];

type AsyncActions =
  | {
      type: "ADD_REDACTION_AND_POTENTIALLY_LOCK";
      payload: {
        pdfId: CaseDocumentViewModel["documentId"];
        redaction: NewPdfHighlight;
      };
    }
  | {
      type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK";
      payload: {
        pdfId: CaseDocumentViewModel["documentId"];
        redactionId: string;
      };
    }
  | {
      type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK";
      payload: {
        pdfId: CaseDocumentViewModel["documentId"];
      };
    }
  | {
      type: "SAVE_REDACTIONS";
      payload: {
        pdfId: CaseDocumentViewModel["documentId"];
      };
    }
  | {
      type: "REQUEST_OPEN_PDF";
      payload: {
        tabSafeId: CaseDocumentViewModel["tabSafeId"];
        pdfId: CaseDocumentViewModel["documentId"];
        mode: CaseDocumentViewModel["mode"];
      };
    }
  | {
      type: "REQUEST_OPEN_PDF_IN_NEW_TAB";
      payload: {
        pdfId: CaseDocumentViewModel["documentId"];
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
        payload: { pdfId },
      } = action;

      const pdfBlobName = getState().tabsState.items.find(
        (item) => item.documentId === pdfId
      )!.pdfBlobName!;

      const sasUrl = await getPdfSasUrl(pdfBlobName);

      dispatch({
        type: "OPEN_PDF_IN_NEW_TAB",
        payload: { pdfId, sasUrl },
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

      const { pdfId } = payload;
      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const { clientLockedState, cmsDocCategory } = items.find(
        (item) => item.documentId === pdfId
      )!;

      const documentRequiresLocking =
        UNLOCKED_STATES_REQUIRING_LOCK.includes(clientLockedState);

      dispatch({ type: "ADD_REDACTION", payload });

      if (!documentRequiresLocking) {
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { pdfId, lockedState: "locking" },
      });

      const isLockSuccessful = await checkoutDocument(
        urn,
        caseId,
        cmsDocCategory,
        pdfId
      );

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: {
          pdfId,
          lockedState: isLockSuccessful ? "locked" : "locked-by-other-user",
        },
      });
    },

  REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;

      const { pdfId } = payload;
      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === pdfId)!;

      const {
        redactionHighlights,
        clientLockedState: lockedState,
        cmsDocCategory,
      } = document;

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
        payload: { pdfId, lockedState: "unlocking" },
      });

      await cancelCheckoutDocument(urn, caseId, cmsDocCategory, pdfId);

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: {
          pdfId,
          lockedState: "unlocked",
        },
      });
    },

  REMOVE_ALL_REDACTIONS_AND_UNLOCK:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;

      const { pdfId } = payload;
      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === pdfId)!;

      const { clientLockedState: lockedState, cmsDocCategory } = document;

      const requiresCheckIn =
        LOCKED_STATES_REQUIRING_UNLOCK.includes(lockedState);

      dispatch({ type: "REMOVE_ALL_REDACTIONS", payload });

      if (!requiresCheckIn) {
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { pdfId, lockedState: "unlocking" },
      });

      await cancelCheckoutDocument(urn, caseId, cmsDocCategory, pdfId);

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: {
          pdfId,
          lockedState: "unlocked",
        },
      });
    },

  SAVE_REDACTIONS:
    ({ getState }) =>
    async (action) => {
      const { payload } = action;
      const { pdfId } = payload;

      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const { redactionHighlights, pdfBlobName, cmsDocCategory } = items.find(
        (item) => item.documentId === pdfId
      )!;

      const redactionSaveRequest = mapRedactionSaveRequest(
        pdfId,
        redactionHighlights
      );

      // todo: make sure UI knows we are saving

      const response = await saveRedactions(
        urn,
        caseId,
        cmsDocCategory,
        pdfId,
        pdfBlobName!, // todo: better typing, but we're guaranteed to have a pdfBlobName anyhow
        redactionSaveRequest
      );

      window.open(response.redactedDocumentUrl);

      // todo: does a save IN THE CGI API check a document in automatically?
      //await cancelCheckoutDocument(urn, caseId, pdfId);

      // todo: make sure UI knows we are saved
    },
};
