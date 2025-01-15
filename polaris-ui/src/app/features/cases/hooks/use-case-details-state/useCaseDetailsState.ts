import { useCallback, useMemo, useEffect } from "react";
import { CombinedState, initialState } from "../../domain/CombinedState";
import { reducer } from "./reducer";
import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import {
  NewPdfHighlight,
  PIIRedactionStatus,
} from "../../domain/NewPdfHighlight";
import { useReducerAsync } from "use-reducer-async";
import { reducerAsyncActionHandlers } from "./reducer-async-action-handlers";
import { useAppInsightsTrackEvent } from "../../../../common/hooks/useAppInsightsTracks";
import { RedactionLogRequestData } from "../../domain/redactionLog/RedactionLogRequestData";
import { RedactionLogTypes } from "../../domain/redactionLog/RedactionLogTypes";
import { TaggedContext } from "../../../../inbound-handover/context";
import { useLoadAppLevelLookups } from "./useLoadAppLevelLookups";
import { useGetCaseData } from "./useGetCaseData";
import { useDocumentSearch } from "./useDocumentSearch";
import { PageDeleteRedaction } from "../../domain/IPageDeleteRedaction";
import { useDocumentRefreshPolling } from "./useDocumentRefreshPolling";
import { useHydrateFromLocalStorage } from "./useHydrateFromLocalStorage";
import { PageRotation } from "../../domain/IPageRotation";
import {
  PresentationDocumentProperties,
  GroupedConversionStatus,
} from "../../domain/gateway/PipelineDocument";
import { useUserGroupsFeatureFlag } from "../../../../auth/msal/useUserGroupsFeatureFlag";
import { getStateFromSessionStorage } from "../../presentation/case-details/utils/stateRetentionUtil";

export type CaseDetailsState = ReturnType<typeof useCaseDetailsState>;

export const useCaseDetailsState = (
  urn: string,
  caseId: number,
  context: TaggedContext | undefined,
  isUnMounting: () => boolean
) => {
  const featureFlagData = useUserGroupsFeatureFlag();
  const retentionState = useMemo(
    () =>
      featureFlagData?.stateRetention ? getStateFromSessionStorage(caseId) : {},
    []
  );
  const trackEvent = useAppInsightsTrackEvent();

  const [combinedState, dispatch] = useReducerAsync(
    reducer,
    {
      ...initialState,
      ...retentionState,
      documentRefreshData: initialState.documentRefreshData,
      searchState: initialState.searchState,
      searchTerm: initialState.searchTerm,
      caseId,
      urn,
      context,
    },
    reducerAsyncActionHandlers
  );

  useLoadAppLevelLookups(dispatch, featureFlagData?.redactionLog);
  useHydrateFromLocalStorage(
    caseId,
    combinedState.storedUserData.status,
    dispatch
  );
  useGetCaseData(urn, caseId, combinedState, dispatch, isUnMounting);
  useDocumentSearch(urn, caseId, combinedState, dispatch);
  useDocumentRefreshPolling(dispatch, combinedState.featureFlags.notifications);

  useEffect(() => {
    dispatch({
      type: "UPDATE_FEATURE_FLAGS_DATA",
      payload: featureFlagData,
    });
  }, [featureFlagData, dispatch]);

  const handleTabSelection = useCallback(
    (documentId: string) => {
      dispatch({
        type: "SET_ACTIVE_TAB",
        payload: {
          pdfId: documentId,
        },
      });
      trackEvent("View Document Tab", { documentId: documentId });
    },
    [dispatch, trackEvent]
  );

  const handleClosePdf = useCallback(
    (documentId: string) => {
      dispatch({
        type: "CLOSE_PDF",
        payload: {
          pdfId: documentId,
        },
      });
      trackEvent("Close Document", { documentId: documentId });
    },
    [dispatch, trackEvent]
  );

  const handleSearchTermChange = useCallback(
    (searchTerm: string) => {
      dispatch({
        type: "UPDATE_SEARCH_TERM",
        payload: {
          searchTerm,
        },
      });
      dispatch({
        type: "UPDATE_PIPELINE_REFRESH",
        payload: {
          startPipelineRefresh: true,
        },
      });
    },
    [dispatch]
  );

  const handleLaunchSearchResults = useCallback(() => {
    dispatch({
      type: "UPDATE_PIPELINE_REFRESH",
      payload: {
        startPipelineRefresh: true,
      },
    });
    dispatch({
      type: "LAUNCH_SEARCH_RESULTS",
    });
  }, [dispatch]);

  const handleCloseSearchResults = useCallback(
    () =>
      dispatch({
        type: "CLOSE_SEARCH_RESULTS",
      }),
    [dispatch]
  );

  const handleChangeResultsOrder = useCallback(
    (newResultsOrder: CombinedState["searchState"]["resultsOrder"]) =>
      dispatch({
        type: "CHANGE_RESULTS_ORDER",
        payload: newResultsOrder,
      }),
    [dispatch]
  );

  const handleUpdateFilter = useCallback(
    (payload: {
      filter: keyof CombinedState["searchState"]["filterOptions"];
      id: string;
      isSelected: boolean;
    }) => dispatch({ type: "UPDATE_FILTER", payload }),
    [dispatch]
  );

  const handleAddRedaction = useCallback(
    (
      documentId: CaseDocumentViewModel["documentId"],
      redactions?: NewPdfHighlight[],
      pageDeleteRedactions?: PageDeleteRedaction[]
    ) =>
      dispatch({
        type: "ADD_REDACTION_OR_ROTATION_AND_POTENTIALLY_LOCK",
        payload: { documentId, redactions, pageDeleteRedactions },
      }),
    [dispatch]
  );

  const handleRemoveRedaction = useCallback(
    (documentId: CaseDocumentViewModel["documentId"], redactionId: string) =>
      dispatch({
        type: "REMOVE_REDACTION_OR_ROTATION_AND_POTENTIALLY_UNLOCK",
        payload: { documentId, redactionId },
      }),
    [dispatch]
  );

  const handleRemoveAllRedactions = useCallback(
    (documentId: CaseDocumentViewModel["documentId"]) =>
      dispatch({
        type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
        payload: { documentId, type: "redaction" },
      }),
    [dispatch]
  );

  const handleSavedRedactions = useCallback(
    (
      documentId: CaseDocumentViewModel["documentId"],
      searchPIIOn: boolean = false
    ) =>
      dispatch({
        type: "SAVE_REDACTIONS",
        payload: { documentId, searchPIIOn },
      }),
    [dispatch]
  );

  const handleSaveRedactionLog = useCallback(
    (
      redactionLogRequestData: RedactionLogRequestData,
      redactionLogType: RedactionLogTypes
    ) =>
      dispatch({
        type: "SAVE_REDACTION_LOG",
        payload: { redactionLogRequestData, redactionLogType },
      }),
    [dispatch]
  );

  const handleCloseErrorModal = useCallback(
    () =>
      dispatch({
        type: "HIDE_ERROR_MODAL",
      }),
    [dispatch]
  );

  const handleShowHideDocumentIssueModal = useCallback(
    (value: boolean) =>
      dispatch({
        type: "SHOW_HIDE_DOCUMENT_ISSUE_MODAL",
        payload: value,
      }),
    [dispatch]
  );

  const handleShowRedactionLogModal = useCallback(
    (type: RedactionLogTypes) =>
      dispatch({
        type: "SHOW_REDACTION_LOG_MODAL",
        payload: {
          type: type,
          savedRedactionTypes: [],
        },
      }),
    [dispatch]
  );

  const handleHideRedactionLogModal = useCallback(
    () =>
      dispatch({
        type: "HIDE_REDACTION_LOG_MODAL",
      }),
    [dispatch]
  );

  const handleUnLockDocuments = useCallback(
    (documentIds: CaseDocumentViewModel["documentId"][]) =>
      dispatch({
        type: "UNLOCK_DOCUMENTS",
        payload: { documentIds },
      }),
    [dispatch]
  );

  const handleAreaOnlyRedaction = useCallback(
    (
      documentId: CaseDocumentViewModel["documentId"],
      enableAreaOnlyMode: boolean
    ) =>
      dispatch({
        type: "ENABLE_AREA_REDACTION_MODE",
        payload: { documentId, enableAreaOnlyMode },
      }),
    [dispatch]
  );

  const handleSaveReadUnreadData = useCallback(
    (documentId) =>
      dispatch({
        type: "SAVE_READ_UNREAD_DATA",
        payload: { documentId },
      }),
    [dispatch]
  );

  const handleOpenPdf = useCallback(
    (caseDocument: {
      documentId: CaseDocumentViewModel["documentId"];
      mode: CaseDocumentViewModel["mode"];
    }) => {
      dispatch({
        type: "REQUEST_OPEN_PDF",
        payload: {
          documentId: caseDocument.documentId,
          mode: caseDocument.mode,
        },
      });
      handleTabSelection(caseDocument.documentId);
      handleSaveReadUnreadData(caseDocument.documentId);
    },
    [dispatch, handleTabSelection, handleSaveReadUnreadData]
  );

  const handleGetNotes = useCallback(
    (documentId: CaseDocumentViewModel["documentId"]) =>
      dispatch({
        type: "GET_NOTES_DATA",
        payload: { documentId },
      }),
    [dispatch]
  );

  const handleAddNote = useCallback(
    (documentId: CaseDocumentViewModel["documentId"], noteText: string) =>
      dispatch({
        type: "ADD_NOTE_DATA",
        payload: {
          documentId,
          noteText,
        },
      }),
    [dispatch]
  );

  const handleSaveRename = useCallback(
    (documentId: CaseDocumentViewModel["documentId"], newName: string) =>
      dispatch({
        type: "SAVE_RENAME_DOCUMENT",
        payload: {
          documentId,
          newName,
        },
      }),
    [dispatch]
  );

  const handleShowHideRedactionSuggestions = useCallback(
    (
      documentId: CaseDocumentViewModel["documentId"],
      versionId: number,
      showSuggestion: boolean,
      getData: boolean,
      defaultOption: boolean
    ) => {
      dispatch({
        type: "SHOW_HIDE_REDACTION_SUGGESTIONS",
        payload: {
          documentId,
          versionId,
          show: showSuggestion,
          getData: getData,
          defaultOption: defaultOption,
        },
      });
      if (getData)
        dispatch({
          type: "GET_SEARCH_PII_DATA",
          payload: {
            documentId,
            versionId,
          },
        });
    },
    [dispatch]
  );

  const handleSearchPIIAction = useCallback(
    (
      documentId: CaseDocumentViewModel["documentId"],
      type: PIIRedactionStatus,
      highlightGroupIds: string[]
    ) => {
      dispatch({
        type: "HANDLE_SEARCH_PII_ACTION",
        payload: {
          documentId,
          type: type,
          highlightGroupIds: highlightGroupIds,
        },
      });
    },
    [dispatch]
  );

  const handleResetRenameData = useCallback(
    (documentId: string) => {
      dispatch({
        type: "UPDATE_RENAME_DATA",
        payload: {
          properties: {
            documentId: documentId,
            newName: "",
            saveRenameRefreshStatus: "initial",
            saveRenameStatus: "initial",
          },
        },
      });
    },
    [dispatch]
  );

  const handleResetReclassifyData = useCallback(
    (documentId: string) => {
      dispatch({
        type: "UPDATE_RECLASSIFY_DATA",
        payload: {
          properties: {
            documentId: documentId,
            saveReclassifyRefreshStatus: "initial",
          },
        },
      });
    },
    [dispatch]
  );

  const handleReclassifySuccess = useCallback(
    (documentId: string, newDocTypeId: number, wasDocumentRenamed: boolean) => {
      dispatch({
        type: "UPDATE_RECLASSIFY_DATA",
        payload: {
          properties: {
            documentId: documentId,
            newDocTypeId: newDocTypeId,
            reclassified: true,
            saveReclassifyRefreshStatus: "updating",
          },
        },
      });

      dispatch({
        type: "REGISTER_NOTIFIABLE_EVENT",
        payload: { documentId, reason: "Reclassified" },
      });

      if (wasDocumentRenamed) {
        dispatch({
          type: "REGISTER_NOTIFIABLE_EVENT",
          payload: { documentId, reason: "Updated" },
        });
      }

      dispatch({
        type: "UPDATE_DOCUMENT_REFRESH",
        payload: {
          startDocumentRefresh: true,
        },
      });
    },
    [dispatch]
  );

  const handleShowHidePageRotation = useCallback(
    (documentId: string, rotatePageMode: boolean) => {
      dispatch({
        type: "SHOW_HIDE_PAGE_ROTATION",
        payload: {
          documentId: documentId,
          rotatePageMode: rotatePageMode,
        },
      });
    },
    [dispatch]
  );

  const handleShowHidePageDeletion = useCallback(
    (documentId: string, deletePageMode: boolean) => {
      dispatch({
        type: "SHOW_HIDE_PAGE_DELETION",
        payload: {
          documentId: documentId,
          deletePageMode: deletePageMode,
        },
      });
    },
    [dispatch]
  );

  const handleAddPageRotation = useCallback(
    (documentId: string, pageRotations: PageRotation[]) => {
      dispatch({
        type: "ADD_REDACTION_OR_ROTATION_AND_POTENTIALLY_LOCK",
        payload: { documentId, pageRotations },
      });
    },
    [dispatch]
  );

  const handleRemovePageRotation = useCallback(
    (documentId: string, rotationId: string) => {
      dispatch({
        type: "REMOVE_REDACTION_OR_ROTATION_AND_POTENTIALLY_UNLOCK",
        payload: { documentId, rotationId },
      });
    },
    [dispatch]
  );

  const handleSaveRotations = useCallback(
    (documentId: CaseDocumentViewModel["documentId"]) =>
      dispatch({
        type: "SAVE_ROTATIONS",
        payload: { documentId },
      }),
    [dispatch]
  );
  const handleRemoveAllRotations = useCallback(
    (documentId: CaseDocumentViewModel["documentId"]) =>
      dispatch({
        type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
        payload: { documentId, type: "rotation" },
      }),
    [dispatch]
  );
  const handleClearAllNotifications = useCallback(
    () =>
      dispatch({
        type: "CLEAR_ALL_NOTIFICATIONS",
      }),
    [dispatch]
  );

  const handleClearNotification = useCallback(
    (notificationId: number) =>
      dispatch({
        type: "CLEAR_NOTIFICATION",
        payload: { notificationId },
      }),
    [dispatch]
  );

  const handleUpdateConversionStatus = useCallback(
    (
      documentId: PresentationDocumentProperties["documentId"],
      status: GroupedConversionStatus
    ) =>
      dispatch({
        type: "UPDATE_CONVERSION_STATUS",
        payload: { documentId, status },
      }),
    [dispatch]
  );

  const handleHideSaveRotationModal = useCallback(
    (documentId: string) =>
      dispatch({
        type: "UPDATE_DOCUMENT_SAVE_STATUS",
        payload: {
          documentId,
          saveStatus: {
            type: "none",
            status: "initial",
          },
        },
      }),
    [dispatch]
  );

  const handleAccordionOpenClose = useCallback(
    (id: string, open: boolean) =>
      dispatch({
        type: "ACCORDION_OPEN_CLOSE",
        payload: { id, open },
      }),
    [dispatch]
  );

  const handleAccordionOpenCloseAll = useCallback(
    (value: boolean) =>
      dispatch({
        type: "ACCORDION_OPEN_CLOSE_ALL",
        payload: value,
      }),
    [dispatch]
  );

  return {
    combinedState,
    handleOpenPdf,
    handleClosePdf,
    handleTabSelection,
    handleSearchTermChange,
    handleLaunchSearchResults,
    handleCloseSearchResults,
    handleChangeResultsOrder,
    handleUpdateFilter,
    handleAddRedaction,
    handleRemoveRedaction,
    handleRemoveAllRedactions,
    handleSavedRedactions,
    handleCloseErrorModal,
    handleUnLockDocuments,
    handleShowHideDocumentIssueModal,
    handleShowRedactionLogModal,
    handleHideRedactionLogModal,
    handleAreaOnlyRedaction,
    handleSaveRedactionLog,
    handleSaveReadUnreadData,
    handleGetNotes,
    handleAddNote,
    handleSaveRename,
    handleShowHideRedactionSuggestions,
    handleSearchPIIAction,
    handleResetRenameData,
    handleReclassifySuccess,
    handleResetReclassifyData,
    handleShowHidePageRotation,
    handleAddPageRotation,
    handleRemovePageRotation,
    handleSaveRotations,
    handleRemoveAllRotations,
    handleClearAllNotifications,
    handleClearNotification,
    handleUpdateConversionStatus,
    handleShowHidePageDeletion,
    handleHideSaveRotationModal,
    handleAccordionOpenClose,
    handleAccordionOpenCloseAll,
  };
};
