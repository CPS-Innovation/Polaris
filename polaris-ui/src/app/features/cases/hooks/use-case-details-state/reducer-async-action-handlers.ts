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

const getOldWorldId = (document: CaseDocumentViewModel) => {
  // temporary function to allow us to use old-world ids for the redaction endpoints
  //  but new-world guid ids for reading and storing documents.
  return document.cmsDocumentId!;
};

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
        tabSafeId: CaseDocumentViewModel["tabSafeId"];
        documentId: CaseDocumentViewModel["documentId"];
        mode: CaseDocumentViewModel["mode"];
      };
    }
  | {
      type: "REQUEST_OPEN_PDF_IN_NEW_TAB";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
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

      const { clientLockedState, cmsDocCategory } = items.find(
        (item) => item.documentId === documentId
      )!;

      const documentRequiresLocking =
        UNLOCKED_STATES_REQUIRING_LOCK.includes(clientLockedState);

      dispatch({ type: "ADD_REDACTION", payload });

      if (!documentRequiresLocking) {
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { documentId, lockedState: "locking" },
      });

      const isLockSuccessful = await checkoutDocument(
        urn,
        caseId,
        cmsDocCategory,
        documentId
      );

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: {
          documentId,
          lockedState: isLockSuccessful ? "locked" : "locked-by-other-user",
        },
      });
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
        payload: { documentId, lockedState: "unlocking" },
      });

      await cancelCheckoutDocument(
        urn,
        caseId,
        cmsDocCategory,
        getOldWorldId(document) // documentId
      );

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

      const { clientLockedState: lockedState, cmsDocCategory } = document;

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

      await cancelCheckoutDocument(
        urn,
        caseId,
        cmsDocCategory,
        getOldWorldId(document) // documentId
      );

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: {
          documentId,
          lockedState: "unlocked",
        },
      });
    },

  SAVE_REDACTIONS:
    ({ getState }) =>
    async (action) => {
      const { payload } = action;
      const { documentId } = payload;

      const {
        tabsState: { items },
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;

      const { redactionHighlights, pdfBlobName, cmsDocCategory } = document;

      const redactionSaveRequest = mapRedactionSaveRequest(
        documentId,
        redactionHighlights
      );

      // todo: make sure UI knows we are saving

      const response = await saveRedactions(
        urn,
        caseId,
        cmsDocCategory,
        getOldWorldId(document), // documentId
        pdfBlobName!, // todo: better typing, but we're guaranteed to have a pdfBlobName anyhow
        redactionSaveRequest
      );

      window.open(response.redactedDocumentUrl);

      // todo: does a save IN THE CGI API check a document in automatically?
      //await cancelCheckoutDocument(urn, caseId, documentId);

      // todo: make sure UI knows we are saved
    },
};
