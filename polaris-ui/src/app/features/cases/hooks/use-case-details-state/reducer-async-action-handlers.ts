import { Reducer } from "react";
import { AsyncActionHandlers } from "use-reducer-async";
import { AsyncResult } from "../../../../common/types/AsyncResult";
import {
  cancelCheckoutDocument,
  checkoutDocument,
  saveRedactions,
  saveRedactionLog,
  getNotesData,
  addNoteData,
  saveDocumentRename,
  getSearchPIIData,
  saveRotations,
  toggleUsedDocumentState,
} from "../../api/gateway-api";
import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { PageDeleteRedaction } from "../../domain/IPageDeleteRedaction";
import { PageRotation, RotationSaveRequest } from "../../domain/IPageRotation";
import {
  mapRedactionSaveRequest,
  mapSearchPIISaveRedactionObject,
} from "./map-redaction-save-request";
import { reducer } from "./reducer";
import { ApiError } from "../../../../common/errors/ApiError";
import { RedactionLogRequestData } from "../../domain/redactionLog/RedactionLogRequestData";
import { RedactionLogTypes } from "../../domain/redactionLog/RedactionLogTypes";
import { addToLocalStorage } from "../../presentation/case-details/utils/localStorageUtils";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { buildHeaders } from "../../api/auth/header-factory";

const LOCKED_STATES_REQUIRING_UNLOCK: CaseDocumentViewModel["clientLockedState"][] =
  ["locked", "locking"];

const UNLOCKED_STATES_REQUIRING_LOCK: CaseDocumentViewModel["clientLockedState"][] =
  ["unlocked", "unlocking", "locked-by-other-user"];

type State = Parameters<typeof reducer>[0];
type Action = Parameters<typeof reducer>[1];

type AsyncActions =
  | {
      type: "ADD_REDACTION_OR_ROTATION_AND_POTENTIALLY_LOCK";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
        redactions?: NewPdfHighlight[];
        pageDeleteRedactions?: PageDeleteRedaction[];
        pageRotations?: PageRotation[];
      };
    }
  | {
      type: "REMOVE_REDACTION_OR_ROTATION_AND_POTENTIALLY_UNLOCK";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
        redactionId?: string;
        rotationId?: string;
      };
    }
  | {
      type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
        type: "redaction" | "rotation";
      };
    }
  | {
      type: "SAVE_REDACTIONS";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
        searchPIIOn: boolean;
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
      type: "SAVE_RENAME_DOCUMENT";
      payload: {
        documentId: string;
        newName: string;
      };
    }
  | {
      type: "GET_SEARCH_PII_DATA";
      payload: {
        documentId: string;
        versionId: number;
      };
    }
  | {
      type: "SAVE_ROTATIONS";
      payload: {
        documentId: CaseDocumentViewModel["documentId"];
      };
    }
  | {
      type: "TOGGLE_DOCUMENT_STATE";
      payload: {
        urn: any;
        caseId: any;
        documentId?: any;
        unused?: any;
      };
    };
// | {
//     type: "DCF_DOCUMENT_VIEW_ACTION_CHANGE";
//     payload: {
//       mode: any;
//     };
//   }
// | {
//     type: "UPDATE_DCF_DOCUMENT_VIEW_ACTION_CHANGE";
//     payload: {
//       mode: any;
//     };
//   };

export const CHECKOUT_BLOCKED_STATUS_CODE = 409;
export const DOCUMENT_NOT_FOUND_STATUS_CODE = 410;
export const DOCUMENT_TOO_LARGE_STATUS_CODE = 413;

const getMappedDocument = (
  documentsState: AsyncResult<MappedCaseDocument[]>,
  documentId: string
) => {
  const documentList =
    documentsState.status === "succeeded" ? documentsState.data : [];
  return documentList.find((item) => item.documentId === documentId)!;
};

export const reducerAsyncActionHandlers: AsyncActionHandlers<
  Reducer<State, Action>,
  AsyncActions
> = {
  REQUEST_OPEN_PDF:
    ({ dispatch }) =>
    async (action) => {
      const {
        payload: { documentId, mode },
      } = action;

      const headers = await buildHeaders();

      dispatch({
        type: "OPEN_PDF",
        payload: { documentId, mode, headers },
      });

      dispatch({
        type: "CLEAR_DOCUMENT_NOTIFICATIONS",
        payload: { documentId },
      });
    },

  ADD_REDACTION_OR_ROTATION_AND_POTENTIALLY_LOCK:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: {
          documentId,
          pageDeleteRedactions,
          redactions,
          pageRotations,
        },
      } = action;
      const {
        tabsState: { items },
        documentsState,
        caseId,
        urn,
      } = getState();

      const { versionId } = getMappedDocument(documentsState, documentId);

      const addRedaction = () => {
        if (pageDeleteRedactions?.length) {
          dispatch({
            type: "ADD_PAGE_DELETE_REDACTION",
            payload: {
              documentId: documentId,
              pageDeleteRedactions: pageDeleteRedactions,
            },
          });
        }
        if (redactions?.length) {
          dispatch({
            type: "ADD_REDACTION",
            payload: {
              documentId: documentId,
              redactions: redactions,
            },
          });
        }
      };

      const addRotation = () => {
        if (pageRotations)
          dispatch({
            type: "ADD_PAGE_ROTATION",
            payload: {
              documentId: documentId,
              pageRotations: pageRotations,
            },
          });
      };

      const { clientLockedState } = items.find(
        (item) => item.documentId === documentId
      )!;

      const documentRequiresLocking =
        UNLOCKED_STATES_REQUIRING_LOCK.includes(clientLockedState);

      if (!documentRequiresLocking) {
        pageRotations ? addRotation() : addRedaction();
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { documentId, lockedState: "locking" },
      });
      try {
        await checkoutDocument(urn, caseId, documentId, versionId);

        pageRotations ? addRotation() : addRedaction();
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

  REMOVE_REDACTION_OR_ROTATION_AND_POTENTIALLY_UNLOCK:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId, redactionId, rotationId },
      } = action;

      const {
        tabsState: { items },
        documentsState,
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;
      const {
        redactionHighlights,
        clientLockedState: lockedState,
        pageDeleteRedactions,
        pageRotations,
      } = document;
      const { versionId } = getMappedDocument(documentsState, documentId);

      if (redactionId) {
        const isRestorePage = pageDeleteRedactions.some(
          (redaction) => redaction.id === redactionId
        );
        if (isRestorePage) {
          dispatch({
            type: "REMOVE_PAGE_DELETE_REDACTION",
            payload: { documentId, redactionId },
          });
        } else {
          dispatch({
            type: "REMOVE_REDACTION",
            payload: { documentId, redactionId },
          });
        }
      }
      if (rotationId) {
        dispatch({
          type: "REMOVE_PAGE_ROTATION",
          payload: {
            documentId,
            rotationId,
          },
        });
      }
      const requiresCheckIn =
        // this is the last existing redaction or rotation
        redactionHighlights.length +
          pageDeleteRedactions.length +
          pageRotations.length ===
          1 && LOCKED_STATES_REQUIRING_UNLOCK.includes(lockedState);

      if (!requiresCheckIn) {
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { documentId, lockedState: "unlocking" },
      });

      await cancelCheckoutDocument(urn, caseId, documentId, versionId);

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
      const {
        payload: { documentId, type },
      } = action;
      const {
        tabsState: { items },
        caseId,
        documentsState,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;

      const { clientLockedState: lockedState } = document;
      const { versionId } = getMappedDocument(documentsState, documentId);

      const requiresCheckIn =
        LOCKED_STATES_REQUIRING_UNLOCK.includes(lockedState);
      type === "redaction"
        ? dispatch({ type: "REMOVE_ALL_REDACTIONS", payload: { documentId } })
        : dispatch({ type: "REMOVE_ALL_ROTATIONS", payload: { documentId } });

      if (!requiresCheckIn) {
        return;
      }

      dispatch({
        type: "UPDATE_DOCUMENT_LOCK_STATE",
        payload: { documentId, lockedState: "unlocking" },
      });

      await cancelCheckoutDocument(urn, caseId, documentId, versionId);

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
      const { documentId, searchPIIOn } = payload;

      const {
        tabsState: { items },
        documentsState,
        caseId,
        urn,
        searchPII,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;
      const { redactionHighlights, pageDeleteRedactions } = document;
      const { versionId } = getMappedDocument(documentsState, documentId);
      let piiData: any = {};
      if (searchPIIOn) {
        const suggestedHighlights =
          searchPII.find((data) => data.documentId === documentId && data.show)
            ?.searchPIIHighlights ?? [];
        piiData = mapSearchPIISaveRedactionObject(
          redactionHighlights,
          suggestedHighlights
        );
      }

      const redactionRequestData = mapRedactionSaveRequest(
        redactionHighlights,
        pageDeleteRedactions
      );

      //piiData is for analytics only
      const redactionSaveRequest = searchPIIOn
        ? { ...redactionRequestData, pii: piiData }
        : redactionRequestData;

      const savedRedactionTypes = [
        ...redactionHighlights,
        ...pageDeleteRedactions,
      ].map((item) => item.redactionType!);
      try {
        dispatch({
          type: "UPDATE_DOCUMENT_SAVE_STATUS",
          payload: {
            documentId,
            saveStatus: { type: "redaction", status: "saving" },
          },
        });
        dispatch({
          type: "SHOW_REDACTION_LOG_MODAL",
          payload: {
            type: RedactionLogTypes.UNDER,
            savedRedactionTypes: savedRedactionTypes,
          },
        });
        await saveRedactions(
          urn,
          caseId,
          documentId,
          versionId,
          redactionSaveRequest
        );
        dispatch({
          type: "UPDATE_DOCUMENT_SAVE_STATUS",
          payload: {
            documentId,
            saveStatus: { type: "redaction", status: "saved" },
          },
        });
        dispatch({
          type: "REMOVE_ALL_REDACTIONS",
          payload: { documentId },
        });
        dispatch({
          type: "SHOW_HIDE_REDACTION_SUGGESTIONS",
          payload: { documentId, versionId, show: false, getData: false },
        });

        dispatch({
          type: "REGISTER_NOTIFIABLE_EVENT",
          payload: { documentId, reason: "New Version" },
        });

        dispatch({
          type: "UPDATE_DOCUMENT_REFRESH",
          payload: {
            startDocumentRefresh: true,
            savedDocumentDetails: {
              documentId,
              versionId,
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
          type: "UPDATE_DOCUMENT_SAVE_STATUS",
          payload: {
            documentId,
            saveStatus: { type: "redaction", status: "error" },
          },
        });
        dispatch({
          type: "HIDE_REDACTION_LOG_MODAL",
        });
      }

      // todo: does a save IN THE CGI API check a document in automatically?
      //await cancelCheckoutDocument(urn, caseId, documentId);
    },

  UNLOCK_DOCUMENTS:
    ({ getState }) =>
    async (action) => {
      const {
        payload: { documentIds },
      } = action;

      const { caseId, urn, documentsState } = getState();

      const items =
        documentsState.status === "succeeded" ? documentsState.data : [];

      const caseIdentifiers = items
        .filter((item) => documentIds.includes(item.documentId))
        .map(({ documentId, versionId }) => ({ documentId, versionId }));

      const requests = caseIdentifiers.map(({ documentId, versionId }) =>
        cancelCheckoutDocument(urn, caseId, documentId, versionId)
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
            getNoteStatus: "success",
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
          type: "UPDATE_DOCUMENT_REFRESH",
          payload: {
            startDocumentRefresh: true,
          },
        });
      }
    },

  SAVE_RENAME_DOCUMENT:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId, newName },
      } = action;
      const { caseId, urn } = getState();
      try {
        dispatch({
          type: "UPDATE_RENAME_DATA",
          payload: {
            properties: {
              documentId,
              newName,
              saveRenameStatus: "saving",
              saveRenameRefreshStatus: "initial",
            },
          },
        });
        await saveDocumentRename(urn, caseId, documentId, newName);
        dispatch({
          type: "UPDATE_RENAME_DATA",
          payload: {
            properties: {
              documentId,
              saveRenameStatus: "success",
              saveRenameRefreshStatus: "updating",
            },
          },
        });

        dispatch({
          type: "REGISTER_NOTIFIABLE_EVENT",
          payload: { documentId, reason: "Updated" },
        });

        dispatch({
          type: "UPDATE_DOCUMENT_REFRESH",
          payload: {
            startDocumentRefresh: true,
          },
        });
      } catch (e) {
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "saverenamedocument",
            title: "Something went wrong!",
            message: "Failed to rename the document. Please try again.",
          },
        });
        dispatch({
          type: "UPDATE_RENAME_DATA",
          payload: {
            properties: {
              documentId,
              saveRenameStatus: "failure",
            },
          },
        });
      }
    },

  TOGGLE_DOCUMENT_STATE:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId, unused },
      } = action;
      const { caseId, urn } = getState();

      try {
        dispatch({
          type: "UPDATE_USED_UNUSED_DOCUMENT",
          payload: {
            documentId,
            saveStatus: "saving",
            saveRefreshStatus: "initial",
          },
        });

        await toggleUsedDocumentState(urn, caseId, documentId, unused);

        dispatch({
          type: "UPDATE_USED_UNUSED_DOCUMENT",
          payload: {
            documentId,
            saveStatus: "success",
            saveRefreshStatus: "updating",
          },
        });

        dispatch({
          type: "UPDATE_DOCUMENT_REFRESH",
          payload: {
            startDocumentRefresh: true,
          },
        });
      } catch (err) {
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "addnote",
            title: "Something went wrong!",
            message: "Failed to change the document state.",
          },
        });
        dispatch({
          type: "UPDATE_USED_UNUSED_DOCUMENT",
          payload: {
            documentId,
            saveStatus: "failure",
            saveRefreshStatus: "updated",
          },
        });
      }
    },

  GET_SEARCH_PII_DATA:
    ({ dispatch, getState }) =>
    async (action) => {
      const {
        payload: { documentId, versionId },
      } = action;
      const { caseId, urn } = getState();
      try {
        const searchPIIResult = await getSearchPIIData(
          urn,
          caseId,
          documentId,
          versionId
        );
        dispatch({
          type: "UPDATE_SEARCH_PII_DATA",
          payload: {
            documentId,
            versionId,
            searchPIIResult,
            getSearchPIIStatus: "success",
          },
        });
      } catch (e) {
        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "getsearchpii",
            title: "Something went wrong!",
            message:
              "Failed to get redaction suggestions for the documents. Please try again.",
          },
        });
        dispatch({
          type: "SHOW_HIDE_REDACTION_SUGGESTIONS",
          payload: { documentId, versionId, show: false, getData: false },
        });
        dispatch({
          type: "UPDATE_SEARCH_PII_DATA",
          payload: {
            documentId,
            versionId,
            searchPIIResult: [],
            getSearchPIIStatus: "failure",
          },
        });
      }
    },
  SAVE_ROTATIONS:
    ({ dispatch, getState }) =>
    async (action) => {
      const { payload } = action;
      const { documentId } = payload;

      const {
        tabsState: { items },
        documentsState,
        caseId,
        urn,
      } = getState();

      const document = items.find((item) => item.documentId === documentId)!;
      const { pageRotations } = document;
      const { versionId } = getMappedDocument(documentsState, documentId);

      const rotationRequestData: RotationSaveRequest = {
        documentModifications: pageRotations
          .filter((data) => data.rotationAngle)
          .map(({ pageNumber, rotationAngle }) => ({
            pageIndex: pageNumber,
            operation: "rotate",
            arg: `${rotationAngle}`,
          })),
      };

      try {
        dispatch({
          type: "UPDATE_DOCUMENT_SAVE_STATUS",
          payload: {
            documentId,
            saveStatus: { type: "rotation", status: "saving" },
          },
        });
        await saveRotations(
          urn,
          caseId,
          documentId,
          versionId,
          rotationRequestData
        );
        dispatch({
          type: "UPDATE_DOCUMENT_SAVE_STATUS",
          payload: {
            documentId,
            saveStatus: { type: "rotation", status: "saved" },
          },
        });
        dispatch({
          type: "REMOVE_ALL_ROTATIONS",
          payload: { documentId },
        });
        dispatch({
          type: "REGISTER_NOTIFIABLE_EVENT",
          payload: { documentId, reason: "New Version" },
        });

        dispatch({
          type: "UPDATE_DOCUMENT_REFRESH",
          payload: {
            startDocumentRefresh: true,
            savedDocumentDetails: {
              documentId,
              versionId,
            },
          },
        });
      } catch (e) {
        const { code } = e as ApiError;
        let errorMessage =
          "Failed to save rotations. Please try again. </p> If re-trying is not successful, please notify the Casework App product team.";

        switch (code) {
          case DOCUMENT_NOT_FOUND_STATUS_CODE:
            errorMessage =
              "Failed to save rotation. The document no longer exists in CMS.";
            break;
          case DOCUMENT_TOO_LARGE_STATUS_CODE:
            errorMessage =
              "Failed to save rotation. The document is too large to rotate.";
            break;
        }

        dispatch({
          type: "SHOW_ERROR_MODAL",
          payload: {
            type: "saverotation",
            title: "Something went wrong!",
            message: errorMessage,
          },
        });
        dispatch({
          type: "UPDATE_DOCUMENT_SAVE_STATUS",
          payload: {
            documentId,
            saveStatus: { type: "rotation", status: "error" },
          },
        });
      }
    },

  // DCF_DOCUMENT_VIEW_ACTION_CHANGE:
  //   ({ dispatch, getState }) =>
  //   async (action) => {
  //     const { mode } = action.payload;
  //     const { caseId, urn } = getState();
  //     try {
  //       dispatch({
  //         type: "UPDATE_DCF_DOCUMENT_VIEW_ACTION_CHANGE",
  //         payload: { mode },
  //       });
  //     } catch (err) {
  //       dispatch({
  //         type: "SHOW_ERROR_MODAL",
  //         payload: {
  //           type: "addnote",
  //           title: "Something went wrong!",
  //           message: "Failed changing mode",
  //         },
  //       });
  //     }
  //     console.log("mode", mode);
  //   },

  // DCF_DOCUMENT_VIEW_ACTION_CHANGE:
  // payload: {
  //   mode: any;
  // };
};
