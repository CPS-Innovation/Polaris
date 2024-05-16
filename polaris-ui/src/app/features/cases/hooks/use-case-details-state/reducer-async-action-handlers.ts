import { Reducer } from "react";
import { AsyncActionHandlers } from "use-reducer-async";
import {
  cancelCheckoutDocument,
  checkoutDocument,
  saveRedactions,
  saveRedactionLog,
  getNotesData,
  addNoteData,
  getSearchPIIData,
} from "../../api/gateway-api";
import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { mapRedactionSaveRequest } from "./map-redaction-save-request";
import { reducer } from "./reducer";
import * as HEADERS from "../../api/header-factory";
import { ApiError } from "../../../../common/errors/ApiError";
import { RedactionLogRequestData } from "../../domain/redactionLog/RedactionLogRequestData";
import { RedactionLogTypes } from "../../domain/redactionLog/RedactionLogTypes";
import { addToLocalStorage } from "../../presentation/case-details/utils/localStorageUtils";

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
        redactions: NewPdfHighlight[];
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
      type: "SAVE_REDACTION_LOG";
      payload: {
        redactionLogRequestData: RedactionLogRequestData;
        redactionLogType: RedactionLogTypes;
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
      type: "UNLOCK_DOCUMENTS";
      payload: {
        documentIds: CaseDocumentViewModel["documentId"][];
      };
    }
  | {
      type: "SAVE_READ_UNREAD_DATA";
      payload: {
        documentId: string;
      };
    }
  | {
      type: "GET_NOTES_DATA";
      payload: {
        documentId: string;
      };
    }
  | {
      type: "ADD_NOTE_DATA";
      payload: {
        documentId: string;
        noteText: string;
      };
    }
  | {
      type: "GET_SEARCH_PII_DATA";
      payload: {
        documentId: string;
      };
    };

export const CHECKOUT_BLOCKED_STATUS_CODE = 409;
export const DOCUMENT_NOT_FOUND_STATUS_CODE = 410;
export const DOCUMENT_TOO_LARGE_STATUS_CODE = 413;

export const reducerAsyncActionHandlers: AsyncActionHandlers<
  Reducer<State, Action>,
  AsyncActions
> = {
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
        const { code, customProperties: { username } = {} } = error as ApiError;

        if (code === CHECKOUT_BLOCKED_STATUS_CODE) {
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
              type: "documentalreadycheckedout",
              title: "Failed to redact document",
              message: `It is not possible to redact as the document is already checked out by ${username}. Please try again later.`,
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
            type: "documentcheckout",
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
      const savedRedactionTypes = redactionHighlights.map(
        (highlight) => highlight.redactionType!
      );
      try {
        dispatch({
          type: "SAVING_REDACTION",
          payload: { documentId, saveStatus: "saving" },
        });
        dispatch({
          type: "SHOW_REDACTION_LOG_MODAL",
          payload: {
            type: RedactionLogTypes.UNDER,
            savedRedactionTypes: savedRedactionTypes,
          },
        });
        await saveRedactions(urn, caseId, documentId, redactionSaveRequest);
        dispatch({
          type: "SAVING_REDACTION",
          payload: { documentId, saveStatus: "saved" },
        });
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
        const { code } = e as ApiError;
        let errorMessage =
          "Failed to save document. Please try again. </p> Your redactions have been saved and it will be possible to re-apply them next time you open this document.</p> If re-trying is not successful, please notify the Casework App product team.";

        switch (code) {
          case DOCUMENT_NOT_FOUND_STATUS_CODE:
            errorMessage =
              "Failed to save redaction. The document no longer exists in CMS.";
            break;
          case DOCUMENT_TOO_LARGE_STATUS_CODE:
            errorMessage =
              "Failed to save redaction. The document is too large to redact.";
            break;
        }

        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "saveredaction",
            title: "Something went wrong!",
            message: errorMessage,
          },
        });
        dispatch({
          type: "SAVING_REDACTION",
          payload: { documentId, saveStatus: "error" },
        });
        dispatch({
          type: "HIDE_REDACTION_LOG_MODAL",
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

  SAVE_REDACTION_LOG:
    ({ dispatch }) =>
    async (action) => {
      const {
        payload: { redactionLogRequestData, redactionLogType },
      } = action;
      try {
        await saveRedactionLog(redactionLogRequestData);

        dispatch({
          type: "HIDE_REDACTION_LOG_MODAL",
        });
      } catch (e) {
        dispatch({
          type: "HIDE_REDACTION_LOG_MODAL",
        });
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "saveredactionlog",
            title: "Something went wrong!",
            message:
              redactionLogType === RedactionLogTypes.UNDER_OVER
                ? "The entries into the Redaction Log have failed. Please try again in the Casework App, or go to the Redaction Log app and enter manually."
                : "The entries into the Redaction Log have failed. Please go to the Redaction Log and enter manually.",
          },
        });
      }
    },

  SAVE_READ_UNREAD_DATA:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;
      const { caseId, storedUserData } = getState();
      if (storedUserData.status !== "succeeded") {
        return;
      }
      if (!storedUserData.data.readUnread.includes(payload.documentId))
        addToLocalStorage(caseId, "readUnread", [
          ...storedUserData.data.readUnread,
          payload.documentId,
        ]);
    },

  GET_NOTES_DATA:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId },
      } = action;
      const { caseId, urn, notes } = getState();
      const isActiveGetNotesRequest =
        notes.find((note) => note.documentId === documentId)?.getNoteStatus ===
        "loading";
      if (isActiveGetNotesRequest) {
        return;
      }
      try {
        dispatch({
          type: "UPDATE_NOTES_DATA",
          payload: {
            documentId,
            notesData: [],
            addNoteStatus: "initial",
            getNoteStatus: "loading",
          },
        });
        const notesData = await getNotesData(urn, caseId, documentId);
        dispatch({
          type: "UPDATE_NOTES_DATA",
          payload: {
            documentId,
            notesData,
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
        });
      } catch (e) {
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "getnotes",
            title: "Something went wrong!",
            message: "Failed to get notes for the documents. Please try again.",
          },
        });
        dispatch({
          type: "UPDATE_NOTES_DATA",
          payload: {
            documentId,
            notesData: [],
            addNoteStatus: "initial",
            getNoteStatus: "failure",
          },
        });
      }
    },

  ADD_NOTE_DATA:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId, noteText },
      } = action;
      const { caseId, urn } = getState();
      let successStatus = true;

      try {
        dispatch({
          type: "UPDATE_NOTES_DATA",
          payload: {
            documentId,
            addNoteStatus: "saving",
            getNoteStatus: "initial",
          },
        });
        await addNoteData(urn, caseId, documentId, noteText);
        dispatch({
          type: "UPDATE_NOTES_DATA",
          payload: {
            documentId,
            addNoteStatus: "success",
            getNoteStatus: "initial",
          },
        });
      } catch (e) {
        successStatus = false;
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "addnote",
            title: "Something went wrong!",
            message: "Failed to add note to the document. Please try again.",
          },
        });
        dispatch({
          type: "UPDATE_NOTES_DATA",
          payload: {
            documentId,
            addNoteStatus: "failure",
            getNoteStatus: "initial",
          },
        });
      }
      if (successStatus) {
        dispatch({
          type: "UPDATE_REFRESH_PIPELINE",
          payload: {
            startRefresh: true,
          },
        });
      }
    },

  GET_SEARCH_PII_DATA:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId },
      } = action;
      const { caseId, urn } = getState();
      try {
        const searchPIIResult = await getSearchPIIData(urn, caseId, documentId);
        dispatch({
          type: "UPDATE_SEARCH_PII_DATA",
          payload: {
            documentId,
            searchPIIResult,
            getSearchPIIStatus: "initial",
          },
        });
      } catch (e) {
        console.log("error>>");
      }
    },
};
