import { ApiResult } from "../../../../common/types/ApiResult";
import { resolvePdfUrl } from "../../api/gateway-api";
import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import { mapAccordionState } from "./map-accordion-state";
import { CombinedState } from "../../domain/CombinedState";
import { CaseDetails } from "../../domain/gateway/CaseDetails";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { ApiTextSearchResult } from "../../domain/gateway/ApiTextSearchResult";
import { mapTextSearch } from "./map-text-search";
import { mapMissingDocuments } from "./map-missing-documents";
import { sortMappedTextSearchResult } from "./sort-mapped-text-search-result";
import { mapDocumentsState } from "./map-documents-state";
import { mapFilters } from "./map-filters";
import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import { isDocumentVisible } from "./is-document-visible";
import { AsyncPipelineResult } from "../use-pipeline-api/AsyncPipelineResult";
import { mapSearchHighlights } from "./map-search-highlights";
import {
  NewPdfHighlight,
  PIIRedactionStatus,
} from "../../domain/NewPdfHighlight";
import { sortSearchHighlights } from "./sort-search-highlights";
import { sanitizeSearchTerm } from "./sanitizeSearchTerm";
import { filterApiResults } from "./filter-api-results";
import { isNewTime, hasDocumentUpdated } from "../utils/refreshUtils";
import { isDocumentsPresentStatus } from "../../domain/gateway/PipelineStatus";
import { SaveStatus } from "../../domain/gateway/SaveStatus";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
  RedactionTypeData,
} from "../../domain/redactionLog/RedactionLogData";
import { FeatureFlagData } from "../../domain/FeatureFlagData";
import { RedactionLogTypes } from "../../domain/redactionLog/RedactionLogTypes";
import {
  addToLocalStorage,
  deleteFromLocalStorage,
} from "../../presentation/case-details/utils/localStorageUtils";
import { getRedactionsToSaveLocally } from "../utils/redactionUtils";
import { StoredUserData } from "../../domain//gateway/StoredUserData";
import { ErrorModalTypes } from "../../domain/ErrorModalTypes";
import { Note } from "../../domain/gateway/NotesData";
import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";
import { SearchPIIResultItem } from "../../domain/gateway/SearchPIIData";
import { mapSearchPIIHighlights } from "../use-case-details-state/map-searchPII-highlights";
import {
  mapNotificationState,
  clearAllNotifications,
  clearNotification,
  readNotification,
  registerNotifiableEvent,
} from "./map-notification-state";
import { NotificationReason } from "../../domain/NotificationState";
import { PageDeleteRedaction } from "../../domain/IPageDeleteRedaction";
import { mapNotificationToDocumentsState } from "./map-notification-to-documents-state";

export type DispatchType = React.Dispatch<Parameters<typeof reducer>["1"]>;

export const reducer = (
  state: CombinedState,
  action:
    | {
        type: "UPDATE_CASE_DETAILS";
        payload: ApiResult<CaseDetails>;
      }
    | {
        type: "UPDATE_PIPELINE";
        payload: AsyncPipelineResult<PipelineResults>;
      }
    | {
        type: "UPDATE_REFRESH_PIPELINE";
        payload: {
          startRefresh: boolean;
          savedDocumentDetails?: {
            documentId: string;
            polarisDocumentVersionId: number;
          };
        };
      }
    | {
        type: "OPEN_PDF";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          mode: CaseDocumentViewModel["mode"];
          headers: HeadersInit;
        };
      }
    | {
        type: "CLOSE_PDF";
        payload: { pdfId: string };
      }
    | {
        type: "SET_ACTIVE_TAB";
        payload: {
          pdfId: string;
        };
      }
    | {
        type: "UPDATE_SEARCH_TERM";
        payload: { searchTerm: string };
      }
    | {
        type: "LAUNCH_SEARCH_RESULTS";
      }
    | {
        type: "UPDATE_SEARCH_RESULTS";
        payload: ApiResult<undefined | ApiTextSearchResult[]>;
      }
    | {
        type: "CLOSE_SEARCH_RESULTS";
      }
    | {
        type: "CHANGE_RESULTS_ORDER";
        payload: CombinedState["searchState"]["resultsOrder"];
      }
    | {
        type: "UPDATE_FILTER";
        payload: {
          filter: keyof CombinedState["searchState"]["filterOptions"];
          id: string;
          isSelected: boolean;
        };
      }
    | {
        type: "ADD_REDACTION";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          redactions: NewPdfHighlight[];
        };
      }
    | {
        type: "ADD_PAGE_DELETE_REDACTION";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          pageDeleteRedactions: PageDeleteRedaction[];
        };
      }
    | {
        type: "SAVING_REDACTION";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          saveStatus: SaveStatus;
        };
      }
    | {
        type: "REMOVE_REDACTION";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          redactionId: string;
        };
      }
    | {
        type: "REMOVE_PAGE_DELETE_REDACTION";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          redactionId: string;
        };
      }
    | {
        type: "REMOVE_ALL_REDACTIONS";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
        };
      }
    | {
        type: "UPDATE_DOCUMENT_LOCK_STATE";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          lockedState: CaseDocumentViewModel["clientLockedState"];
        };
      }
    | {
        type: "SHOW_ERROR_MODAL";
        payload: {
          message: string;
          title: string;
          type: ErrorModalTypes;
        };
      }
    | {
        type: "HIDE_ERROR_MODAL";
      }
    | {
        type: "SHOW_HIDE_DOCUMENT_ISSUE_MODAL";
        payload: boolean;
      }
    | {
        type: "SHOW_REDACTION_LOG_MODAL";
        payload: {
          type: RedactionLogTypes;
          savedRedactionTypes: RedactionTypeData[];
        };
      }
    | {
        type: "HIDE_REDACTION_LOG_MODAL";
      }
    | {
        type: "UPDATE_REDACTION_LOG_LOOK_UPS_DATA";
        payload: ApiResult<RedactionLogLookUpsData>;
      }
    | {
        type: "UPDATE_REDACTION_LOG_MAPPING_DATA";
        payload: ApiResult<RedactionLogMappingData>;
      }
    | {
        type: "UPDATE_FEATURE_FLAGS_DATA";
        payload: FeatureFlagData;
      }
    | {
        type: "ENABLE_AREA_REDACTION_MODE";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          enableAreaOnlyMode: boolean;
        };
      }
    | {
        type: "UPDATE_STORED_USER_DATA";
        payload: {
          storedUserData: StoredUserData;
        };
      }
    | {
        type: "UPDATE_NOTES_DATA";
        payload:
          | {
              documentId: string;
              addNoteStatus: "saving" | "failure" | "success";
              getNoteStatus: "initial";
            }
          | {
              documentId: string;
              notesData: Note[];
              addNoteStatus: "initial";
              getNoteStatus: "initial" | "loading" | "failure";
            };
      }
    | {
        type: "UPDATE_RENAME_DATA";
        payload: {
          properties:
            | {
                documentId: string;
                saveRenameStatus?: "failure" | "success";
                saveRenameRefreshStatus?: "updating" | "updated";
              }
            | {
                documentId: string;
                newName: string;
                saveRenameStatus: "saving" | "initial";
                saveRenameRefreshStatus: "initial";
              };
        };
      }
    | {
        type: "UPDATE_RECLASSIFY_DATA";
        payload: {
          properties: {
            documentId: string;
            newDocTypeId?: number;
            reclassified?: boolean;
            saveReclassifyRefreshStatus: "initial" | "updating" | "updated";
          };
        };
      }
    | {
        type: "SHOW_HIDE_REDACTION_SUGGESTIONS";
        payload: {
          documentId: string;
          show: boolean;
          getData: boolean;
          defaultOption?: boolean;
        };
      }
    | {
        type: "UPDATE_SEARCH_PII_DATA";
        payload: {
          documentId: string;
          searchPIIResult: SearchPIIResultItem[];
          getSearchPIIStatus: "initial" | "failure" | "loading" | "success";
        };
      }
    | {
        type: "HANDLE_SEARCH_PII_ACTION";
        payload: {
          documentId: string;
          highlightGroupIds: string[];
          type: PIIRedactionStatus;
        };
      }
    | {
        type: "REGISTER_NOTIFIABLE_EVENT";
        payload: { documentId: string; reason: NotificationReason };
      }
    | {
        type: "CLEAR_ALL_NOTIFICATIONS";
      }
    | {
        type: "CLEAR_NOTIFICATION";
        payload: { notificationId: number };
      }
    | {
        type: "READ_NOTIFICATION";
        payload: { notificationId: number };
      }
): CombinedState => {
  switch (action.type) {
    case "UPDATE_CASE_DETAILS":
      if (action.payload.status === "failed") {
        throw action.payload.error;
      }

      return { ...state, caseState: action.payload };

    case "UPDATE_REDACTION_LOG_LOOK_UPS_DATA":
      if (action.payload.status === "failed") {
        return state;
      }
      return {
        ...state,
        redactionLog: {
          ...state.redactionLog,
          redactionLogLookUpsData: action.payload,
        },
      };

    case "UPDATE_REDACTION_LOG_MAPPING_DATA":
      if (action.payload.status === "failed") {
        return state;
      }
      return {
        ...state,
        redactionLog: {
          ...state.redactionLog,
          redactionLogMappingData: action.payload,
        },
      };

    case "UPDATE_PIPELINE": {
      if (action.payload.status === "failed") {
        throw action.payload.error;
      }

      if (action.payload.status === "initiating") {
        return state;
      }

      let nextState = { ...state };

      if (action.payload.data.status === "Completed") {
        const newPipelineData = action.payload.data;

        nextState = {
          ...nextState,
          pipelineRefreshData: {
            ...nextState.pipelineRefreshData,
            // If a document that is lined up to be saved and has been updated
            //  then we need to drop it - no sense in updating it.
            savedDocumentDetails:
              nextState.pipelineRefreshData.savedDocumentDetails.filter(
                (document) => !hasDocumentUpdated(document, newPipelineData)
              ),
            lastProcessingCompleted: newPipelineData.processingCompleted,
          },
        };
      }

      const shouldBuildDocumentsState =
        isDocumentsPresentStatus(action.payload.data.status) &&
        isNewTime(
          action.payload.data.documentsRetrieved,
          (state.pipelineState.haveData &&
            state.pipelineState.data.documentsRetrieved) ||
            ""
        );

      if (shouldBuildDocumentsState) {
        const coreDocumentsState = mapDocumentsState(
          action.payload.data.documents,
          (state.caseState &&
            state.caseState.status === "succeeded" &&
            state.caseState.data.witnesses) ||
            []
        );

        const notificationState = mapNotificationState(
          state.notificationState,
          state.documentsState,
          coreDocumentsState,
          action.payload.data.documentsRetrieved
        );

        const documentsState = mapNotificationToDocumentsState(
          notificationState,
          coreDocumentsState
        );

        const accordionState = mapAccordionState(documentsState);

        nextState = {
          ...nextState,
          notificationState,
          documentsState,
          accordionState,
        };
      }

      const newPipelineResults = action.payload;

      const coreNextPipelineState = {
        ...nextState,
        pipelineState: {
          ...newPipelineResults,
        },
      };

      const deletedOpenPDfsTabs = state.tabsState.items.filter(
        (item) =>
          !newPipelineResults.data.documents.some(
            (document) => document.documentId === item.documentId
          )
      );

      const openPdfsWeNeedToUpdate = newPipelineResults.data.documents
        .filter(
          (item) =>
            item.pdfBlobName &&
            state.tabsState.items.some(
              (tabItem) => tabItem.documentId === item.documentId
            )
        )
        .map(
          ({
            documentId,
            pdfBlobName,
            polarisDocumentVersionId,
            presentationTitle,
          }) => ({
            documentId,
            pdfBlobName,
            polarisDocumentVersionId,
            presentationTitle,
          })
        );
      if (!openPdfsWeNeedToUpdate.length) {
        return coreNextPipelineState;
      }

      /*
        Note: if we are looking for open tabs that do not yet know their url (i.e. the
          user has opened a document from the accordion before the pipeline has given us
          the blob name for that document), it can only be after the document has been
          launched from the accordion.  This means we don't have to worry about search
          highlighting from this point on, only setting the URL (i.e. the document will be in 
          "read" mode, not "search" mode)
      */

      const nextOpenTabs = state.tabsState.items.reduce((prev, curr) => {
        const matchingFreshPdfRecord = openPdfsWeNeedToUpdate.find(
          (item) => item.documentId === curr.documentId
        );

        if (matchingFreshPdfRecord) {
          const url = resolvePdfUrl(
            state.urn,
            state.caseId,
            matchingFreshPdfRecord.documentId,
            matchingFreshPdfRecord.polarisDocumentVersionId
          );
          return [
            ...prev,
            {
              ...curr,
              url,
              pdfBlobName: matchingFreshPdfRecord.pdfBlobName,
              polarisDocumentVersionId:
                matchingFreshPdfRecord.polarisDocumentVersionId,
              presentationFileName: matchingFreshPdfRecord.presentationTitle,
            },
          ];
        }

        //update the open tabs with deleted documents
        const matchingDeletedPdfRecords = deletedOpenPDfsTabs.find(
          (item) => item.documentId === curr.documentId
        );
        if (matchingDeletedPdfRecords) {
          return [...prev, { ...curr, isDeleted: true }];
        }

        return [...prev, curr];
      }, [] as CaseDocumentViewModel[]);

      return {
        ...coreNextPipelineState,
        tabsState: { ...state.tabsState, items: nextOpenTabs },
      };
    }

    case "UPDATE_REFRESH_PIPELINE": {
      const {
        savedDocumentDetails: payloadSavedDocumentDetails,
        startRefresh,
      } = action.payload;

      const savedDocumentDetails = payloadSavedDocumentDetails
        ? [
            ...state.pipelineRefreshData.savedDocumentDetails,
            payloadSavedDocumentDetails,
          ]
        : state.pipelineRefreshData.savedDocumentDetails;

      return {
        ...state,
        pipelineRefreshData: {
          ...state.pipelineRefreshData,
          startRefresh,
          savedDocumentDetails,
        },
      };
    }

    case "OPEN_PDF":
      const { documentId, mode, headers } = action.payload;

      const coreNewState = {
        ...state,
        searchState: {
          ...state.searchState,
          isResultsVisible: false,
        },
        tabsState: {
          ...state.tabsState,
          headers,
        },
      };

      if (state.documentsState.status !== "succeeded") {
        // this is just here to keep typing happy, it is not logically
        //  possible to be opening a pdf without the documents call
        //  having already completed.
        return coreNewState;
      }

      const isTabAlreadyOpenedInRequiredState = state.tabsState.items.some(
        (item) =>
          item.documentId === documentId &&
          // we have found the tab already exists in read mode and we are trying to
          //  open again in read mode
          ((item.mode === "read" && mode === "read") ||
            // we have found the tab open in search mode and we are trying to open again
            //  with the exact same search term
            (item.mode === "search" &&
              mode === "search" &&
              item.searchTerm === state.searchState.submittedSearchTerm))
      );

      if (isTabAlreadyOpenedInRequiredState) {
        // there is nothing more to do, the tab control will show the appropriate tab
        //  via the url hash functionality
        return coreNewState;
      }

      const alreadyOpenedTabIndex = state.tabsState.items.findIndex(
        (item) => item.documentId === documentId
      );

      const redactionsHighlightsToRetain =
        alreadyOpenedTabIndex !== -1
          ? state.tabsState.items[alreadyOpenedTabIndex].redactionHighlights
          : [];

      const foundDocument = state.documentsState.data.find(
        (item) => item.documentId === documentId
      )!;

      const pipelineDocument = state.pipelineState.haveData
        ? state.pipelineState.data.documents.find(
            (item) => item.documentId === documentId
          )
        : undefined;

      const blobName = pipelineDocument?.pdfBlobName;

      const url =
        blobName &&
        resolvePdfUrl(
          state.urn,
          state.caseId,
          pipelineDocument.documentId,
          pipelineDocument.polarisDocumentVersionId
        );

      let item: CaseDocumentViewModel;

      const coreItem = {
        ...foundDocument,
        clientLockedState: "unlocked" as const,
        url,
        pdfBlobName: blobName,
        redactionHighlights: redactionsHighlightsToRetain,
        pageDeleteRedactions: [],
        isDeleted: false,
        saveStatus: "initial" as const,
      };

      if (mode === "read") {
        item = {
          ...coreItem,
          sasUrl: undefined,
          mode: "read",
          areaOnlyRedactionMode: false,
        };
      } else {
        const foundDocumentSearchResult =
          state.searchState.results.status === "succeeded" &&
          state.searchState.results.data.documentResults.find(
            (item) => item.documentId === documentId
          )!;

        const pageOccurrences = foundDocumentSearchResult
          ? foundDocumentSearchResult.occurrences.reduce(
              (
                acc,
                { pageIndex, pageHeight, pageWidth, occurrencesInLine }
              ) => {
                let foundPage = acc.find(
                  (item) => item.pageIndex === pageIndex
                );

                if (!foundPage) {
                  foundPage = {
                    pageIndex,
                    pageHeight,
                    pageWidth,
                    boundingBoxes: [],
                  };
                  acc.push(foundPage);
                }

                foundPage.boundingBoxes = [
                  ...foundPage.boundingBoxes,
                  ...occurrencesInLine,
                ];

                return acc;
              },
              [] as {
                pageIndex: number;
                pageHeight: number;
                pageWidth: number;
                boundingBoxes: number[][];
              }[]
            )
          : /* istanbul ignore next */ [];

        const unsortedSearchHighlights = mapSearchHighlights(pageOccurrences);

        const sortedHighlights = sortSearchHighlights(unsortedSearchHighlights);

        item = {
          ...coreItem,
          mode: "search",
          sasUrl: undefined,
          searchTerm: state.searchState.submittedSearchTerm!,
          occurrencesInDocumentCount: foundDocumentSearchResult
            ? foundDocumentSearchResult.occurrencesInDocumentCount
            : /* istanbul ignore next */ 0,
          searchHighlights: sortedHighlights,
          areaOnlyRedactionMode: false,
        };
      }

      const nextItemsArray =
        alreadyOpenedTabIndex === -1
          ? // this is the first time we are opening this tab
            [...state.tabsState.items, item]
          : // this is a subsequent time, and the tab is now different (maybe going from
            //  read to search mode or maybe a different search term)
            state.tabsState.items.map((existingItem, index) =>
              index === alreadyOpenedTabIndex
                ? { ...existingItem, ...item }
                : existingItem
            );

      const isUnread =
        state.storedUserData?.status === "succeeded" &&
        !state.storedUserData?.data.readUnread.includes(documentId);

      return {
        ...coreNewState,
        tabsState: {
          ...coreNewState.tabsState,
          items: nextItemsArray,
        },
        searchState: {
          ...state.searchState,
          isResultsVisible: false,
        },
        ...(isUnread && state.storedUserData?.status === "succeeded"
          ? {
              storedUserData: {
                ...state.storedUserData,
                data: {
                  ...state.storedUserData.data,
                  readUnread: [
                    ...state.storedUserData.data.readUnread,
                    documentId,
                  ],
                },
              },
            }
          : {}),
      };

    case "CLOSE_PDF": {
      const { pdfId } = action.payload;

      return {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.filter(
            (item) => item.documentId !== pdfId
          ),
        },
      };
    }

    case "SET_ACTIVE_TAB": {
      const { pdfId } = action.payload;
      return {
        ...state,
        tabsState: {
          ...state.tabsState,
          activeTabId: pdfId,
        },
      };
    }
    case "UPDATE_SEARCH_TERM":
      return {
        ...state,
        searchTerm: action.payload.searchTerm,
      };
    case "CLOSE_SEARCH_RESULTS":
      return {
        ...state,
        searchState: {
          ...state.searchState,
          isResultsVisible: false,
        },
      };
    case "LAUNCH_SEARCH_RESULTS":
      const { searchState, searchTerm } = state;
      const requestedSearchTerm = searchTerm.trim();
      const submittedSearchTerm = sanitizeSearchTerm(requestedSearchTerm);

      return {
        ...state,
        searchState: {
          ...searchState,
          isResultsVisible: true,
          requestedSearchTerm,
          submittedSearchTerm,
        },
      };

    case "UPDATE_SEARCH_RESULTS":
      if (action.payload.status === "failed") {
        throw action.payload.error;
      }

      if (action.payload.status === "loading") {
        return {
          ...state,
          searchState: {
            ...state.searchState,
            results: { status: "loading" },
          },
        };
      }

      if (
        state.documentsState.status === "succeeded" &&
        state.pipelineState.status === "complete" &&
        state.searchState.submittedSearchTerm &&
        action.payload.data
      ) {
        const filteredSearchResults = filterApiResults(
          action.payload.data,
          state.documentsState.data
        );

        const unsortedData = mapTextSearch(
          filteredSearchResults,
          state.documentsState.data
        );

        const sortedData = sortMappedTextSearchResult(
          unsortedData,
          state.searchState.resultsOrder
        );

        const missingDocs = mapMissingDocuments(
          state.pipelineState.data,
          state.documentsState.data
        );

        const filterOptions = mapFilters(unsortedData);

        return {
          ...state,
          searchState: {
            ...state.searchState,
            missingDocs,
            filterOptions,
            results: {
              status: "succeeded",
              data: sortedData,
            },
          },
        };
      }

      return state;

    case "CHANGE_RESULTS_ORDER":
      return {
        ...state,
        searchState: {
          ...state.searchState,
          resultsOrder: action.payload,
          results:
            state.searchState.results.status === "loading"
              ? // if loading, then there are no stable results to search,
                //  also required for type checking :)
                state.searchState.results
              : {
                  ...state.searchState.results,
                  data: sortMappedTextSearchResult(
                    state.searchState.results.data,
                    action.payload
                  ),
                },
        },
      };

    case "UPDATE_FILTER":
      const { isSelected, filter, id } = action.payload;

      const nextState = {
        ...state,
        searchState: {
          ...state.searchState,
          filterOptions: {
            ...state.searchState.filterOptions,
            [filter]: {
              ...state.searchState.filterOptions[filter],
              [id]: {
                ...state.searchState.filterOptions[filter][id],
                isSelected,
              },
            },
          },
        },
      };

      if (state.searchState.results.status !== "succeeded") {
        return nextState;
      }

      const nextResults = state.searchState.results.data.documentResults.reduce(
        (acc, curr) => {
          const { isVisible, hasChanged } = isDocumentVisible(
            curr,
            nextState.searchState.filterOptions
          );

          acc.push(hasChanged ? { ...curr, isVisible } : curr);
          return acc;
        },
        [] as MappedDocumentResult[]
      );

      const { filteredDocumentCount, filteredOccurrencesCount } =
        nextResults.reduce(
          (acc, curr) => {
            if (curr.isVisible) {
              acc.filteredDocumentCount += 1;
              acc.filteredOccurrencesCount += curr.occurrencesInDocumentCount;
            }

            return acc;
          },
          { filteredDocumentCount: 0, filteredOccurrencesCount: 0 }
        );

      return {
        ...nextState,
        searchState: {
          ...nextState.searchState,
          results: {
            ...state.searchState.results,
            data: {
              ...state.searchState.results.data,
              documentResults: nextResults,
              filteredDocumentCount,
              filteredOccurrencesCount,
            },
          },
        },
      };
    case "ADD_REDACTION": {
      const { documentId, redactions } = action.payload;

      const newRedactions = redactions.map((redaction, index) => ({
        ...redaction,
        id: String(`${+new Date()}-${index}`),
      }));

      let newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  redactionHighlights: [
                    ...item.redactionHighlights,
                    ...newRedactions,
                  ],
                }
              : item
          ),
        },
      };
      //adding redactions to local storage
      const redactionsToSave = getRedactionsToSaveLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );
      if (redactionsToSave.length) {
        addToLocalStorage(state.caseId, "redactions", redactionsToSave);
      }
      return newState;
    }
    case "ADD_PAGE_DELETE_REDACTION": {
      const { documentId, pageDeleteRedactions } = action.payload;
      const newRedactions = pageDeleteRedactions.map((redaction, index) => ({
        ...redaction,
        id: String(`${+new Date()}-${index}`),
      }));

      //This is applicable only when the user deletes a page with unsaved redactions
      const clearPageUnsavedRedactions = (
        redactionHighlights: IPdfHighlight[]
      ) => {
        if (pageDeleteRedactions.length > 1) return [];
        return redactionHighlights.filter(
          (redaction) =>
            redaction?.position?.pageNumber !==
            pageDeleteRedactions[0].pageNumber
        );
      };

      let newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  pageDeleteRedactions: [
                    ...item.pageDeleteRedactions,
                    ...newRedactions,
                  ],
                  redactionHighlights: [
                    ...clearPageUnsavedRedactions(item.redactionHighlights),
                  ],
                }
              : item
          ),
        },
      };
      //adding redactions to local storage
      const redactionsToSave = getRedactionsToSaveLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );
      if (redactionsToSave.length) {
        addToLocalStorage(state.caseId, "redactions", redactionsToSave);
      }
      return newState;
    }
    case "SAVING_REDACTION": {
      const { documentId, saveStatus } = action.payload;

      return {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  saveStatus: saveStatus,
                }
              : item
          ),
        },
      };
    }
    case "REMOVE_REDACTION": {
      const { redactionId, documentId } = action.payload;

      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  redactionHighlights: item.redactionHighlights.filter(
                    (redaction) => redaction.id !== redactionId
                  ),
                }
              : item
          ),
        },
      };
      //adding redactions to local storage
      const redactionsToSave = getRedactionsToSaveLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );
      redactionsToSave.length
        ? addToLocalStorage(state.caseId, "redactions", redactionsToSave)
        : deleteFromLocalStorage(state.caseId, "redactions");

      return newState;
    }

    case "REMOVE_PAGE_DELETE_REDACTION": {
      const { redactionId, documentId } = action.payload;

      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  pageDeleteRedactions: item.pageDeleteRedactions.filter(
                    (redaction) => redaction.id !== redactionId
                  ),
                }
              : item
          ),
        },
      };
      //adding redactions to local storage
      const redactionsToSave = getRedactionsToSaveLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );
      redactionsToSave.length
        ? addToLocalStorage(state.caseId, "redactions", redactionsToSave)
        : deleteFromLocalStorage(state.caseId, "redactions");

      return newState;
    }

    case "REMOVE_ALL_REDACTIONS": {
      const { documentId } = action.payload;
      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  redactionHighlights: [],
                  pageDeleteRedactions: [],
                }
              : item
          ),
        },
      };
      //adding redaction highlight to local storage
      const redactionHighlights = getRedactionsToSaveLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );
      redactionHighlights.length
        ? addToLocalStorage(state.caseId, "redactions", redactionHighlights)
        : deleteFromLocalStorage(state.caseId, "redactions");

      return newState;
    }
    case "UPDATE_DOCUMENT_LOCK_STATE": {
      const { documentId, lockedState } = action.payload;

      return {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  clientLockedState: lockedState,
                }
              : item
          ),
        },
      };
    }
    case "SHOW_ERROR_MODAL": {
      const { message, title, type } = action.payload;
      return {
        ...state,
        errorModal: {
          show: true,
          message: message,
          title: title,
          type: type,
        },
      };
    }
    case "HIDE_ERROR_MODAL": {
      return {
        ...state,
        errorModal: {
          show: false,
          message: "",
          title: "",
          type: "",
        },
      };
    }
    case "SHOW_HIDE_DOCUMENT_ISSUE_MODAL": {
      return {
        ...state,
        documentIssueModal: {
          show: action.payload,
        },
      };
    }
    case "SHOW_REDACTION_LOG_MODAL": {
      return {
        ...state,
        redactionLog: {
          ...state.redactionLog,
          showModal: true,
          type: action.payload.type,
          savedRedactionTypes: action.payload.savedRedactionTypes,
        },
      };
    }
    case "HIDE_REDACTION_LOG_MODAL": {
      return {
        ...state,
        redactionLog: {
          ...state.redactionLog,
          showModal: false,
          savedRedactionTypes: [],
        },
      };
    }

    case "UPDATE_FEATURE_FLAGS_DATA": {
      return {
        ...state,
        featureFlags: action.payload,
      };
    }
    case "ENABLE_AREA_REDACTION_MODE": {
      const { documentId, enableAreaOnlyMode } = action.payload;
      return {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  areaOnlyRedactionMode: enableAreaOnlyMode,
                }
              : item
          ),
        },
      };
    }
    case "UPDATE_STORED_USER_DATA": {
      const { storedUserData } = action.payload;
      return {
        ...state,
        storedUserData: { status: "succeeded", data: storedUserData },
      };
    }
    case "UPDATE_NOTES_DATA": {
      const { documentId, addNoteStatus, getNoteStatus } = action.payload;
      const filteredNotes = state.notes.filter(
        (note) => note.documentId !== documentId
      );
      const activeNotes = state.notes.find(
        (note) => note.documentId === documentId
      )!;
      switch (addNoteStatus) {
        case "success":
        case "failure":
        case "saving": {
          return {
            ...state,
            notes: [
              ...filteredNotes,
              {
                ...activeNotes,
                documentId,
                addNoteStatus,
                getNoteStatus,
              },
            ],
          };
        }
        default:
          const { notesData } = action.payload;
          return {
            ...state,
            notes: [
              ...filteredNotes,
              {
                documentId,
                notes: notesData,
                addNoteStatus,
                getNoteStatus,
              },
            ],
          };
      }
    }

    case "UPDATE_RENAME_DATA": {
      const { properties } = action.payload;
      const filteredData = state.renameDocuments.filter(
        (data) => data.documentId !== properties.documentId
      );
      let currentData = state.renameDocuments.find(
        (data) => data.documentId === properties.documentId
      )!;

      if (properties.saveRenameStatus === "saving") {
        return {
          ...state,
          renameDocuments: [
            ...filteredData,
            {
              ...properties,
            },
          ],
        };
      }

      return {
        ...state,
        renameDocuments: [
          ...filteredData,
          {
            ...currentData,
            ...properties,
          },
        ],
      };
    }

    case "UPDATE_RECLASSIFY_DATA": {
      const { properties } = action.payload;

      const filteredData = state.reclassifyDocuments.filter(
        (data) => data.documentId !== properties.documentId
      );
      let currentData = state.reclassifyDocuments.find(
        (data) => data.documentId === properties.documentId
      )!;

      return {
        ...state,
        reclassifyDocuments: [
          ...filteredData,
          {
            ...currentData,
            ...properties,
          },
        ],
      };
    }

    case "SHOW_HIDE_REDACTION_SUGGESTIONS": {
      const {
        documentId,
        show,
        getData,
        defaultOption = true,
      } = action.payload;
      const polarisDocumentVersionId = state.tabsState.items.find(
        (data) => data.documentId === documentId
      )?.polarisDocumentVersionId!;
      const availablePIIData = state.searchPII.find(
        (data) => data.documentId === documentId
      );
      const newSearchPIIHighlights =
        availablePIIData?.searchPIIHighlights.map((highlight) => ({
          ...highlight,
          redactionStatus:
            highlight.redactionStatus === "ignored" ||
            highlight.redactionStatus === "ignoredAll"
              ? ("initial" as const)
              : highlight.redactionStatus,
        })) ?? [];

      const newData = availablePIIData
        ? {
            ...availablePIIData,
            show: show,
            defaultOption: defaultOption,
            searchPIIHighlights: getData ? [] : newSearchPIIHighlights,
            polarisDocumentVersionId: getData
              ? polarisDocumentVersionId
              : availablePIIData.polarisDocumentVersionId,
          }
        : {
            show: show,
            defaultOption: defaultOption,
            documentId: documentId,
            polarisDocumentVersionId: polarisDocumentVersionId,
            searchPIIHighlights: [],
            getSearchPIIStatus: "initial" as const,
          };

      return {
        ...state,
        searchPII: [
          ...state.searchPII.filter((data) => data.documentId !== documentId),
          newData,
        ],
      };
    }

    case "UPDATE_SEARCH_PII_DATA": {
      const { documentId, searchPIIResult, getSearchPIIStatus } =
        action.payload;
      const filteredSearchPIIDatas = state.searchPII.filter(
        (searchPIIResult) => searchPIIResult.documentId !== documentId
      );
      const activeSearchPIIData = state.searchPII.find(
        (searchPIIResult) => searchPIIResult.documentId === documentId
      )!;

      const missedRedactionTypesData =
        state.redactionLog.redactionLogLookUpsData.status === "succeeded"
          ? state.redactionLog.redactionLogLookUpsData.data.missedRedactions
          : [];

      const searchPIIHighlights = mapSearchPIIHighlights(
        searchPIIResult,
        missedRedactionTypesData
      );

      const sortedSearchPIIHighlights =
        sortSearchHighlights(searchPIIHighlights);
      return {
        ...state,
        searchPII: [
          ...filteredSearchPIIDatas,
          {
            ...activeSearchPIIData,
            documentId,
            searchPIIHighlights: sortedSearchPIIHighlights,
            getSearchPIIStatus: getSearchPIIStatus,
          },
        ],
      };
    }

    case "HANDLE_SEARCH_PII_ACTION": {
      const { documentId, type, highlightGroupIds } = action.payload;
      const filteredSearchPIIDatas = state.searchPII.filter(
        (searchPIIResult) => searchPIIResult.documentId !== documentId
      );

      const searchPIIDataItem = state.searchPII?.find(
        (searchPIIDataItem) => searchPIIDataItem.documentId === documentId
      )!;

      let textContent: string = "";
      let selectedHighlights: {
        selected: ISearchPIIHighlight[];
        rest: ISearchPIIHighlight[];
      } = { selected: [], rest: [] };
      const isTextMatchAction = type === "ignoredAll" || type === "acceptedAll";
      if (isTextMatchAction) {
        textContent =
          searchPIIDataItem.searchPIIHighlights.find(
            (highlight) => highlight.groupId === highlightGroupIds[0]
          )?.textContent ?? "";
      }
      selectedHighlights = searchPIIDataItem.searchPIIHighlights.reduce(
        (acc, highlight) => {
          if (
            isTextMatchAction &&
            highlight.textContent === textContent &&
            highlight.redactionStatus === "initial"
          ) {
            acc.selected.push(highlight);
            return acc;
          }
          if (
            !isTextMatchAction &&
            highlightGroupIds.includes(highlight.groupId)
          ) {
            acc.selected.push(highlight);
            return acc;
          }

          acc.rest.push(highlight);
          return acc;
        },
        {
          selected: [] as ISearchPIIHighlight[],
          rest: [] as ISearchPIIHighlight[],
        }
      );

      const newHighlights = selectedHighlights.selected.map((highlight) => {
        highlight.redactionStatus = type;
        return highlight;
      });

      const newState = {
        ...state,
        searchPII: [
          ...filteredSearchPIIDatas,
          {
            ...searchPIIDataItem,
            searchPIIHighlights: [...selectedHighlights.rest, ...newHighlights],
          },
        ],
      };

      return newState;
    }

    case "REGISTER_NOTIFIABLE_EVENT": {
      return {
        ...state,
        notificationState: registerNotifiableEvent(
          state.notificationState,
          action.payload
        ),
      };
    }

    case "CLEAR_ALL_NOTIFICATIONS": {
      const notificationState = clearAllNotifications(state.notificationState);
      const documentsState = mapNotificationToDocumentsState(
        notificationState,
        state.documentsState
      );
      const accordionState = mapAccordionState(documentsState);

      return {
        ...state,
        notificationState,
        documentsState,
        accordionState,
      };
    }

    case "CLEAR_NOTIFICATION": {
      const notificationState = clearNotification(
        state.notificationState,
        action.payload.notificationId
      );
      const documentsState = mapNotificationToDocumentsState(
        notificationState,
        state.documentsState
      );
      const accordionState = mapAccordionState(documentsState);

      return {
        ...state,
        notificationState,
        documentsState,
        accordionState,
      };
    }

    case "READ_NOTIFICATION": {
      return {
        ...state,
        notificationState: readNotification(
          state.notificationState,
          action.payload.notificationId
        ),
      };
    }
    default:
      throw new Error("Unknown action passed to case details reducer");
  }
};
