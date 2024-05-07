import { useCallback, useEffect } from "react";
import { useApi } from "../../../../common/hooks/useApi";
import { useUserDetails } from "../../../../auth";
import {
  getCaseDetails,
  searchCase,
  getRedactionLogLookUpsData,
  getRedactionLogMappingData,
} from "../../api/gateway-api";
import { usePipelineApi } from "../use-pipeline-api/usePipelineApi";
import { CombinedState } from "../../domain/CombinedState";
import { reducer } from "./reducer";
import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { useReducerAsync } from "use-reducer-async";
import { reducerAsyncActionHandlers } from "./reducer-async-action-handlers";
import { useAppInsightsTrackEvent } from "../../../../common/hooks/useAppInsightsTracks";
import { RedactionLogRequestData } from "../../domain/redactionLog/RedactionLogRequestData";
import { useUserGroupsFeatureFlag } from "../../../../auth/msal/useUserGroupsFeatureFlag";
import { RedactionLogTypes } from "../../domain/redactionLog/RedactionLogTypes";
import {
  readFromLocalStorage,
  ReadUnreadData,
} from "../../presentation/case-details/utils/localStorageUtils";

export type CaseDetailsState = ReturnType<typeof useCaseDetailsState>;

export const initialState = {
  caseState: { status: "loading" },
  documentsState: { status: "loading" },
  pipelineState: { status: "initiating", haveData: false, correlationId: "" },
  pipelineRefreshData: {
    startRefresh: true,
    savedDocumentDetails: [],
    lastProcessingCompleted: "",
  },
  accordionState: { status: "loading" },
  tabsState: { items: [], headers: {}, activeTabId: undefined },
  searchTerm: "",
  searchState: {
    isResultsVisible: false,
    requestedSearchTerm: undefined,
    submittedSearchTerm: undefined,
    resultsOrder: "byDateDesc",
    filterOptions: {
      docType: {},
      category: {},
    },
    missingDocs: [],
    results: { status: "loading" },
  },
  errorModal: {
    show: false,
    message: "",
    title: "",
    type: "",
  },
  confirmationModal: {
    show: false,
    message: "",
  },
  documentIssueModal: {
    show: false,
  },
  redactionLog: {
    showModal: false,
    type: RedactionLogTypes.UNDER,
    redactionLogLookUpsData: { status: "loading" },
    redactionLogMappingData: { status: "loading" },
    savedRedactionTypes: [],
  },
  featureFlags: {
    redactionLog: false,
    fullScreen: false,
    notes: false,
    searchPII: false,
  },
  storedUserData: { status: "loading" },
  notes: [],
  pIIData: [],
} as Omit<CombinedState, "caseId" | "urn">;

export const useCaseDetailsState = (urn: string, caseId: number) => {
  const userDetails = useUserDetails();
  const featureFlagData = useUserGroupsFeatureFlag();
  const caseState = useApi(getCaseDetails, [urn, caseId]);
  const trackEvent = useAppInsightsTrackEvent();

  const [combinedState, dispatch] = useReducerAsync(
    reducer,
    { ...initialState, caseId, urn },
    reducerAsyncActionHandlers
  );

  const pipelineState = usePipelineApi(
    urn,
    caseId,
    combinedState.pipelineRefreshData
  );

  const redactionLogLookUpsData = useApi(
    getRedactionLogLookUpsData,
    [],
    combinedState.featureFlags.redactionLog
  );

  const redactionLogMappingData = useApi(
    getRedactionLogMappingData,
    [],
    combinedState.featureFlags.redactionLog
  );

  useEffect(() => {
    if (combinedState.storedUserData.status === "loading") {
      const docReadData = readFromLocalStorage(
        caseId,
        "readUnread"
      ) as ReadUnreadData | null;
      dispatch({
        type: "UPDATE_STORED_USER_DATA",
        payload: {
          storedUserData: { readUnread: docReadData ?? [] },
        },
      });
    }
  }, [combinedState.storedUserData.status, caseId, dispatch]);

  useEffect(() => {
    if (redactionLogLookUpsData.status !== "initial")
      dispatch({
        type: "UPDATE_REDACTION_LOG_LOOK_UPS_DATA",
        payload: redactionLogLookUpsData,
      });
  }, [redactionLogLookUpsData, dispatch]);

  useEffect(() => {
    if (redactionLogMappingData.status !== "initial")
      dispatch({
        type: "UPDATE_REDACTION_LOG_MAPPING_DATA",
        payload: redactionLogMappingData,
      });
  }, [redactionLogMappingData, dispatch]);

  useEffect(() => {
    dispatch({
      type: "UPDATE_FEATURE_FLAGS_DATA",
      payload: featureFlagData,
    });
  }, [featureFlagData, dispatch]);

  useEffect(() => {
    if (caseState.status !== "initial")
      dispatch({ type: "UPDATE_CASE_DETAILS", payload: caseState });
  }, [caseState, dispatch]);

  useEffect(() => {
    dispatch({ type: "UPDATE_PIPELINE", payload: pipelineState });
  }, [pipelineState, dispatch]);

  useEffect(() => {
    const { startRefresh } = combinedState.pipelineRefreshData;
    if (startRefresh) {
      dispatch({
        type: "UPDATE_REFRESH_PIPELINE",
        payload: {
          startRefresh: false,
        },
      });
    }
  }, [combinedState.pipelineRefreshData, dispatch]);

  const searchResults = useApi(
    searchCase,
    [
      urn,
      caseId,
      combinedState.searchState.submittedSearchTerm
        ? combinedState.searchState.submittedSearchTerm
        : "",
    ],
    //  Note: we let the user trigger a search without the pipeline being ready.
    //  If we additionally observe the complete-state of the pipeline here, we can ensure that a search
    //  is triggered when either:
    //  a) the pipeline is ready and the user subsequently submits a search
    //  b) the user submits a search before the pipeline is ready, but it then becomes ready
    // combinedState.pipelineState.status === "complete",
    //  It makes it much easier if we enforce that the documents need to be known before allowing
    //   a search (logically, we do not need to wait for the documents call to return at the point we trigger a
    //   search, we only need them when we map the eventual result of the search call).  However, this is a tidier
    //   place to enforce the wait as we are already waiting for the pipeline here. If we don't wait here, then
    //   we have to deal with the condition where the search results have come back but we do not yet have the
    //   the documents result, and we have to chase up fixing the full mapped objects at that later point.
    //   (Assumption: this is edge-casey stuff as the documents call should always really have come back unless
    //   the user is super quick to trigger a search).
    !!(
      combinedState.searchState.submittedSearchTerm &&
      combinedState.pipelineState.status === "complete" &&
      combinedState.documentsState.status === "succeeded"
    )
  );

  useEffect(() => {
    if (searchResults.status !== "initial") {
      dispatch({ type: "UPDATE_SEARCH_RESULTS", payload: searchResults });
    }
  }, [searchResults, dispatch]);

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
    [dispatch]
  );

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
    [dispatch]
  );

  const handleClosePdf = useCallback(
    (caseDocument: { documentId: string }) => {
      dispatch({
        type: "CLOSE_PDF",
        payload: {
          pdfId: caseDocument.documentId,
        },
      });
      trackEvent("Close Document", { documentId: caseDocument.documentId });
    },
    [dispatch]
  );

  const handleSearchTermChange = useCallback(
    (searchTerm: string) => {
      dispatch({
        type: "UPDATE_SEARCH_TERM",
        payload: {
          searchTerm,
        },
      });
    },
    [dispatch]
  );

  const handleLaunchSearchResults = useCallback(
    () =>
      dispatch({
        type: "LAUNCH_SEARCH_RESULTS",
      }),
    [dispatch]
  );

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
      redactions: NewPdfHighlight[]
    ) =>
      dispatch({
        type: "ADD_REDACTION_AND_POTENTIALLY_LOCK",
        payload: { documentId, redactions },
      }),
    [dispatch]
  );

  const handleRemoveRedaction = useCallback(
    (documentId: CaseDocumentViewModel["documentId"], redactionId: string) =>
      dispatch({
        type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK",
        payload: { documentId, redactionId },
      }),
    [dispatch]
  );

  const handleRemoveAllRedactions = useCallback(
    (documentId: CaseDocumentViewModel["documentId"]) =>
      dispatch({
        type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
        payload: { documentId },
      }),
    [dispatch]
  );

  const handleSavedRedactions = useCallback(
    (documentId: CaseDocumentViewModel["documentId"]) =>
      dispatch({ type: "SAVE_REDACTIONS", payload: { documentId } }),
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

  const handleShowHideRedactionSuggestions = useCallback(
    (
      documentId: CaseDocumentViewModel["documentId"],
      showSuggestion: boolean
    ) =>
      dispatch({
        type: "SHOW_HIDE_REDACTION_SUGGESTIONS",
        payload: {
          documentId,
          show: showSuggestion,
        },
      }),
    [dispatch]
  );

  return {
    ...combinedState,
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
    handleShowHideRedactionSuggestions,
  };
};
