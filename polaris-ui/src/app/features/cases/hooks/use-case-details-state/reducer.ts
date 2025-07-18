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
import { hasDocumentUpdated } from "../utils/refreshUtils";
import { SaveStatus } from "../../domain/gateway/SaveStatus";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
  RedactionTypeData,
} from "../../domain/redactionLog/RedactionLogData";
import { FeatureFlagData } from "../../domain/FeatureFlagData";
import { RedactionLogTypes } from "../../domain/redactionLog/RedactionLogTypes";
import { handleSaveRedactionsLocally } from "../utils/redactionUtils";
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
  registerNotifiableEvent,
  clearDocumentNotifications,
} from "./map-notification-state";
import { NotificationReason } from "../../domain/NotificationState";
import {
  PageDeleteRedaction,
  IPageDeleteRedaction,
} from "../../domain/IPageDeleteRedaction";
import { PageRotation, IPageRotation } from "../../domain/IPageRotation";
import { mapNotificationToDocumentsState } from "./map-notification-to-documents-state";
import {
  PresentationDocumentProperties,
  GroupedConversionStatus,
} from "../../domain/gateway/PipelineDocument";
import { LocalDocumentState } from "../../domain/LocalDocumentState";
import { shouldTriggerPipelineRefresh } from "../utils/shouldTriggerPipelineRefresh";
import { mapDocumentNameSearch } from "./map-document-name-search";
import { combineDocumentNameMatches } from "./combine-document-name-matches";

export type DispatchType = React.Dispatch<Parameters<typeof reducer>["1"]>;

export const reducer = (
  state: CombinedState,
  action:
    | {
        type: "UPDATE_CASE_DETAILS";
        payload: ApiResult<CaseDetails>;
      }
    | {
        type: "UPDATE_DOCUMENTS";
        payload: ApiResult<PresentationDocumentProperties[]>;
      }
    | {
        type: "UPDATE_PIPELINE";
        payload: AsyncPipelineResult<PipelineResults>;
      }
    | {
        type: "UPDATE_DOCUMENT_REFRESH";
        payload: {
          startDocumentRefresh: boolean;
          savedDocumentDetails?: {
            documentId: string;
            versionId: number;
          };
        };
      }
    | {
        type: "UPDATE_PIPELINE_REFRESH";
        payload: {
          startPipelineRefresh: boolean;
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
        type: "UPDATE_SEARCH_TYPE";
        payload: CombinedState["searchState"]["searchType"];
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
        payload: CombinedState["searchState"]["searchConfigs"]["documentContent"]["resultsOrder"];
      }
    | {
        type: "UPDATE_FILTER";
        payload: {
          filter: keyof CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"];
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
        type: "UPDATE_DOCUMENT_SAVE_STATUS";
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
              getNoteStatus: "initial" | "success" | "loading" | "failure";
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
          properties:
            | {
                documentId: string;
                saveReclassifyRefreshStatus: "initial" | "updated";
              }
            | {
                documentId: string;
                newDocTypeId: number;
                reclassified: boolean;
                saveReclassifyRefreshStatus: "updating";
              };
        };
      }
    | {
        type: "SHOW_HIDE_REDACTION_SUGGESTIONS";
        payload: {
          documentId: string;
          versionId: number;
          show: boolean;
          getData: boolean;
          defaultOption?: boolean;
        };
      }
    | {
        type: "UPDATE_SEARCH_PII_DATA";
        payload: {
          documentId: string;
          versionId: number;
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
        type: "CLEAR_DOCUMENT_NOTIFICATIONS";
        payload: { documentId: string };
      }
    | {
        type: "SHOW_HIDE_PAGE_ROTATION";
        payload: { documentId: string; rotatePageMode: boolean };
      }
    | {
        type: "SHOW_HIDE_PAGE_DELETION";
        payload: { documentId: string; deletePageMode: boolean };
      }
    | {
        type: "ADD_PAGE_ROTATION";
        payload: { documentId: string; pageRotations: PageRotation[] };
      }
    | {
        type: "REMOVE_PAGE_ROTATION";
        payload: {
          documentId: string;
          rotationId: string;
        };
      }
    | {
        type: "REMOVE_ALL_ROTATIONS";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
        };
      }
    | {
        type: "UPDATE_CONVERSION_STATUS";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          status: GroupedConversionStatus;
        };
      }
    | {
        type: "ACCORDION_OPEN_CLOSE";
        payload: { id: string; open: boolean };
      }
    | { type: "ACCORDION_OPEN_CLOSE_ALL"; payload: boolean }
    | {
        type: "UPDATE_USED_UNUSED_DOCUMENT";
        payload: {
          documentId: CaseDocumentViewModel["documentId"];
          saveStatus: "initial" | "saving" | "success" | "failure";
          saveRefreshStatus: "initial" | "updating" | "updated";
        };
      }
    | {
        type: "DCF_DOCUMENT_VIEW_ACTION_CHANGE";
        payload: {
          dcfMode: string | undefined;
        };
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

    case "UPDATE_DOCUMENTS": {
      const { payload } = action;
      if (payload.status === "failed") {
        throw payload.error;
      }

      if (payload.status === "loading") {
        return state;
      }

      const { data } = payload;
      let nextState = { ...state };

      const coreDocumentsState = mapDocumentsState(
        data,
        (state.caseState?.status === "succeeded" &&
          state.caseState.data.witnesses) ||
          []
      );

      const notificationState = mapNotificationState(
        state.notificationState,
        state.documentsState,
        coreDocumentsState,
        new Date().toISOString()
      );

      const documentsState = mapNotificationToDocumentsState(
        notificationState,
        coreDocumentsState
      );

      const accordionState = mapAccordionState(
        documentsState,
        state.accordionState
      );

      nextState = {
        ...nextState,
        notificationState,
        documentsState,
        accordionState,
        documentRefreshData: {
          ...nextState.documentRefreshData,
          savedDocumentDetails:
            nextState.documentRefreshData.savedDocumentDetails.filter(
              (document) => !hasDocumentUpdated(document, data)
            ),
        },
      };

      //Todo: Move this whole update of the open tabs into its own util function
      const openPdfsWeNeedToUpdate = data
        .filter((item) =>
          state.tabsState.items.some(
            (tabItem) => tabItem.documentId === item.documentId
          )
        )
        .map(
          ({ documentId, versionId, presentationTitle, isOcrProcessed }) => ({
            documentId,
            versionId,
            presentationTitle,
            isOcrProcessed,
          })
        );
      if (!openPdfsWeNeedToUpdate.length) {
        return nextState;
      }

      const deletedOpenPDfsTabs = state.tabsState.items.filter(
        (item) =>
          !data.some((document) => document.documentId === item.documentId)
      );

      // Note: if we are looking for open tabs that do not yet know their url (i.e. the
      // user has opened a document from the accordion before the pipeline has given us
      // the blob name for that document), it can only be after the document has been
      // launched from the accordion.  This means we don't have to worry about search
      // highlighting from this point on, only setting the URL (i.e. the document will be in
      // "read" mode, not "search" mode)

      const nextOpenTabs = state.tabsState.items.reduce((prev, curr) => {
        const matchingFreshPdfRecord = openPdfsWeNeedToUpdate.find(
          (item) => item.documentId === curr.documentId
        );

        if (matchingFreshPdfRecord) {
          const url = resolvePdfUrl(
            state.urn,
            state.caseId,
            matchingFreshPdfRecord.documentId,
            matchingFreshPdfRecord.versionId,
            matchingFreshPdfRecord.isOcrProcessed
          );
          return [
            ...prev,
            {
              ...curr,
              url,
              versionId: matchingFreshPdfRecord.versionId,
              presentationTitle: matchingFreshPdfRecord.presentationTitle,
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
        ...nextState,
        tabsState: { ...state.tabsState, items: nextOpenTabs },
      };
    }

    case "UPDATE_PIPELINE": {
      if (action.payload.status === "failed") {
        throw action.payload.error;
      }

      if (action.payload.status === "initiating") {
        return {
          ...state,
          pipelineState: {
            ...state.pipelineState,
            status: "initiating",
          },
        };
      }
      //temporary mapping until the mock data is cleaned
      const mappedData = {
        ...action.payload,
        data: {
          ...action.payload.data,
          documents: action.payload.data.documents.map((doc) => ({
            documentId: doc.documentId,
            status: doc.status,
            conversionStatus: doc.conversionStatus,
          })),
        },
      };

      const newLocalDocumentState = action.payload.data.documents.reduce(
        (acc, curr) => {
          acc[`${curr.documentId}`] = {
            conversionStatus: curr.conversionStatus,
          };
          return acc;
        },
        {} as LocalDocumentState
      );
      return {
        ...state,
        pipelineState: {
          ...state.pipelineState,
          ...(mappedData as AsyncPipelineResult<PipelineResults>),
        },
        pipelineRefreshData: {
          ...state.pipelineRefreshData,
          lastProcessingCompleted: action.payload.data.processingCompleted, //may be can use it from pipelineState
          localLastRefreshTime:
            action.payload.data.status === "Completed"
              ? new Date().toISOString()
              : state.pipelineRefreshData.localLastRefreshTime,
        },
        localDocumentState: newLocalDocumentState,
      };
    }

    case "UPDATE_DOCUMENT_REFRESH": {
      const {
        savedDocumentDetails: payloadSavedDocumentDetails,
        startDocumentRefresh,
      } = action.payload;

      const savedDocumentDetails = payloadSavedDocumentDetails
        ? [
            ...state.documentRefreshData.savedDocumentDetails,
            payloadSavedDocumentDetails,
          ]
        : state.documentRefreshData.savedDocumentDetails;

      return {
        ...state,
        documentRefreshData: {
          ...state.documentRefreshData,
          startDocumentRefresh,
          savedDocumentDetails,
        },
      };
    }
    case "UPDATE_PIPELINE_REFRESH": {
      const { startPipelineRefresh } = action.payload;

      return {
        ...state,
        pipelineRefreshData: {
          ...state.pipelineRefreshData,
          startPipelineRefresh,
        },
      };
    }

    case "OPEN_PDF": {
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

      const document = !!state.documentsState.data
        ? state.documentsState.data.find(
            (item) => item.documentId === documentId
          )
        : undefined;

      const url =
        document &&
        resolvePdfUrl(
          state.urn,
          state.caseId,
          document.documentId,
          document.versionId,
          document.isOcrProcessed
        );

      let item: CaseDocumentViewModel;

      const coreItem = {
        ...foundDocument,
        clientLockedState: "unlocked" as const,
        url,
        redactionHighlights: redactionsHighlightsToRetain,
        pageDeleteRedactions: [],
        pageRotations: [],
        rotatePageMode: false,
        deletePageMode: true,
        isDeleted: false,
        saveStatus: { type: "none", status: "initial" } as SaveStatus,
      };

      if (mode === "read") {
        item = {
          ...coreItem,
          mode: "read",
          areaOnlyRedactionMode: false,
        };
      } else {
        const foundDocumentSearchResult =
          state.searchState.searchConfigs["documentContent"].results.status ===
          "succeeded"
            ? state.searchState.searchConfigs[
                "documentContent"
              ].results.data.documentResults.find(
                (item) => item.documentId === documentId
              )
            : undefined;

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
    }

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
        searchState: {
          ...state.searchState,
          lastSubmittedSearchTerm: state.searchState.submittedSearchTerm,
        },
      };

    case "UPDATE_SEARCH_TYPE":
      return {
        ...state,
        searchState: {
          ...state.searchState,
          searchType: action.payload,
        },
      };

    case "CLOSE_SEARCH_RESULTS":
      return {
        ...state,
        searchState: {
          ...state.searchState,
          isResultsVisible: false,
        },
      };

    case "LAUNCH_SEARCH_RESULTS": {
      const shouldWaitForNewPipelineRefresh = shouldTriggerPipelineRefresh(
        state.notificationState?.lastModifiedDateTime ?? "",
        state.pipelineRefreshData?.localLastRefreshTime
      );
      const { searchState, searchTerm } = state;
      const requestedSearchTerm = searchTerm.trim();
      const submittedSearchTerm = sanitizeSearchTerm(requestedSearchTerm);

      if (state.documentsState.status === "succeeded") {
        const unsortedData = mapDocumentNameSearch(
          submittedSearchTerm,
          state.documentsState.data
        );

        const sortedData = sortMappedTextSearchResult(
          unsortedData,
          state.searchState.searchConfigs.documentName.resultsOrder
        );

        const filterOptions = mapFilters(unsortedData);

        return {
          ...state,
          searchState: {
            ...searchState,
            isResultsVisible: true,
            requestedSearchTerm,
            submittedSearchTerm,
            lastSubmittedSearchTerm: shouldWaitForNewPipelineRefresh
              ? ""
              : state.searchState.submittedSearchTerm ?? "",
            searchConfigs: {
              ...state.searchState.searchConfigs,
              documentName: {
                resultsOrder: "byDateDesc",
                filterOptions,
                results: {
                  status: "succeeded",
                  data: sortedData,
                },
              },
            },
          },
        };
      }

      return state;
    }

    case "UPDATE_SEARCH_RESULTS": {
      if (action.payload.status === "failed") {
        throw action.payload.error;
      }

      if (action.payload.status === "loading") {
        return {
          ...state,
          searchState: {
            ...state.searchState,
            searchConfigs: {
              ...state.searchState.searchConfigs,
              documentContent: {
                ...state.searchState.searchConfigs.documentContent,
                results: { status: "loading" },
              },
            },
          },
        };
      }

      if (
        state.documentsState.status === "succeeded" &&
        state.pipelineState.status === "complete" &&
        state.searchState.submittedSearchTerm &&
        state.searchState.searchConfigs.documentName.results.status ===
          "succeeded" &&
        action.payload.data
      ) {
        const filteredSearchResults = filterApiResults(
          action.payload.data,
          state.documentsState.data
        );

        const textSearchResults = mapTextSearch(
          filteredSearchResults,
          state.documentsState.data
        );

        const unsortedData = combineDocumentNameMatches(
          textSearchResults,
          state.searchState.searchConfigs.documentName.results.data
            .documentResults,
          state.featureFlags.documentNameSearch
        );

        const sortedData = sortMappedTextSearchResult(
          unsortedData,
          state.searchState.searchConfigs.documentContent.resultsOrder
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
            searchConfigs: {
              ...state.searchState.searchConfigs,
              documentContent: {
                ...state.searchState.searchConfigs.documentContent,
                filterOptions,
                results: {
                  status: "succeeded",
                  data: sortedData,
                },
              },
            },
          },
        };
      }

      return state;
    }

    case "CHANGE_RESULTS_ORDER": {
      const updateResultsOrder = (
        searchType: CombinedState["searchState"]["searchType"]
      ) => {
        const results = state.searchState.searchConfigs[searchType].results;

        return {
          ...state,
          searchState: {
            ...state.searchState,
            searchConfigs: {
              ...state.searchState.searchConfigs,
              [searchType]: {
                ...state.searchState.searchConfigs[searchType],
                resultsOrder: action.payload,
                results:
                  results.status === "loading"
                    ? results
                    : {
                        ...results,
                        data: sortMappedTextSearchResult(
                          results.data,
                          action.payload
                        ),
                      },
              },
            },
          },
        };
      };

      if (state.searchState.searchType === "documentContent") {
        return updateResultsOrder("documentContent");
      }

      return updateResultsOrder("documentName");
    }

    case "UPDATE_FILTER": {
      const { isSelected, filter, id } = action.payload;

      const updateFilterOptions = (
        searchType: CombinedState["searchState"]["searchType"]
      ) => ({
        ...state,
        searchState: {
          ...state.searchState,
          searchConfigs: {
            ...state.searchState.searchConfigs,
            [searchType]: {
              ...state.searchState.searchConfigs[searchType],
              filterOptions: {
                ...state.searchState.searchConfigs[searchType].filterOptions,
                [filter]: {
                  ...state.searchState.searchConfigs[searchType].filterOptions[
                    filter
                  ],
                  [id]: {
                    ...state.searchState.searchConfigs[searchType]
                      .filterOptions[filter][id],
                    isSelected,
                  },
                },
              },
            },
          },
        },
      });

      const updateResults = (
        nextState: typeof state,
        searchType: CombinedState["searchState"]["searchType"]
      ) => {
        const results = nextState.searchState.searchConfigs[searchType].results;

        if (results.status !== "succeeded") {
          return nextState;
        }

        const nextResults = results.data.documentResults.reduce((acc, curr) => {
          const { isVisible, hasChanged } = isDocumentVisible(
            curr,
            nextState.searchState.searchConfigs[searchType].filterOptions
          );

          acc.push(hasChanged ? { ...curr, isVisible } : curr);
          return acc;
        }, [] as MappedDocumentResult[]);

        const { filteredDocumentCount, filteredOccurrencesCount } =
          nextResults.reduce(
            (acc, curr) => {
              if (curr.isVisible) {
                acc.filteredDocumentCount += 1;
                acc.filteredOccurrencesCount += curr.occurrencesInDocumentCount;
                if (
                  curr.isDocumentNameMatch &&
                  state.featureFlags.documentNameSearch
                ) {
                  acc.filteredOccurrencesCount += 1;
                }
              }
              return acc;
            },
            { filteredDocumentCount: 0, filteredOccurrencesCount: 0 }
          );

        return {
          ...nextState,
          searchState: {
            ...nextState.searchState,
            searchConfigs: {
              ...nextState.searchState.searchConfigs,
              [searchType]: {
                ...nextState.searchState.searchConfigs[searchType],
                results: {
                  ...results,
                  data: {
                    ...results.data,
                    documentResults: nextResults,
                    filteredDocumentCount,
                    filteredOccurrencesCount,
                  },
                },
              },
            },
          },
        };
      };

      const searchType = state.searchState.searchType;
      const nextState = updateFilterOptions(searchType);

      if (
        state.searchState.searchConfigs[searchType].results.status !==
        "succeeded"
      ) {
        return nextState;
      }

      return updateResults(nextState, searchType);
    }

    case "ADD_REDACTION": {
      const { documentId, redactions } = action.payload;

      const newRedactions = redactions.map((redaction, index) => ({
        ...redaction,
        id: String(`${+new Date()}-${index}`),
      }));

      const newState = {
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
      handleSaveRedactionsLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );

      return newState;
    }
    case "ADD_PAGE_DELETE_REDACTION": {
      const { documentId, pageDeleteRedactions } = action.payload;
      const newRedactions = pageDeleteRedactions.map((redaction, index) => ({
        ...redaction,
        id: String(`${+new Date()}-${index}`),
      }));

      //Bug:28212 - This is a fix for bug which we could not reproduce, by filtering out any duplicate page delete entries
      const filterDuplicates = (
        pageDeleteRedactions: IPageDeleteRedaction[]
      ) => {
        const deletedPages = new Set();

        return pageDeleteRedactions.filter((redaction) => {
          if (!deletedPages.has(redaction.pageNumber)) {
            deletedPages.add(redaction.pageNumber);
            return true;
          }
          return false;
        });
      };

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

      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  pageDeleteRedactions: filterDuplicates([
                    ...item.pageDeleteRedactions,
                    ...newRedactions,
                  ]),
                  redactionHighlights: [
                    ...clearPageUnsavedRedactions(item.redactionHighlights),
                  ],
                }
              : item
          ),
        },
      };
      //adding redactions to local storage
      handleSaveRedactionsLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );
      return newState;
    }
    case "UPDATE_DOCUMENT_SAVE_STATUS": {
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
      handleSaveRedactionsLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );

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
      handleSaveRedactionsLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );

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
      handleSaveRedactionsLocally(
        newState.tabsState.items,
        documentId,
        state.caseId
      );

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
        default: {
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
    }

    case "UPDATE_RENAME_DATA": {
      const { properties } = action.payload;
      const filteredData = state.renameDocuments.filter(
        (data) => data.documentId !== properties.documentId
      );
      const currentData = state.renameDocuments.find(
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
      const currentData = state.reclassifyDocuments.find(
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
        versionId,
        show,
        getData,
        defaultOption = true,
      } = action.payload;
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
          }
        : {
            show: show,
            defaultOption: defaultOption,
            documentId: documentId,
            versionId,
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
      const { documentId, versionId, searchPIIResult, getSearchPIIStatus } =
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
            versionId,
            searchPIIHighlights: sortedSearchPIIHighlights,
            getSearchPIIStatus: getSearchPIIStatus,
          },
        ],
      };
    }

    case "HANDLE_SEARCH_PII_ACTION": {
      const { documentId, type, highlightGroupIds } = action.payload;
      const filteredSearchPIIDatas = state.searchPII?.filter(
        (searchPIIResult) => searchPIIResult.documentId !== documentId
      );

      const searchPIIDataItem = state.searchPII.find(
        (searchPIIDataItem) => searchPIIDataItem?.documentId === documentId
      )!;

      let textContent = "";
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

      return {
        ...state,
        notificationState,
        documentsState,
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

      return {
        ...state,
        notificationState,
        documentsState,
      };
    }
    case "CLEAR_DOCUMENT_NOTIFICATIONS": {
      const notificationState = clearDocumentNotifications(
        state.notificationState,
        action.payload.documentId
      );
      const documentsState = mapNotificationToDocumentsState(
        notificationState,
        state.documentsState
      );

      return {
        ...state,
        notificationState,
        documentsState,
      };
    }

    case "SHOW_HIDE_PAGE_DELETION": {
      const { documentId, deletePageMode } = action.payload;

      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  deletePageMode: deletePageMode,
                  rotatePageMode: deletePageMode ? false : item.rotatePageMode,
                }
              : item
          ),
        },
      };
      return newState;
    }

    case "SHOW_HIDE_PAGE_ROTATION": {
      const { documentId, rotatePageMode } = action.payload;

      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  rotatePageMode: rotatePageMode,
                }
              : item
          ),
        },
      };
      return newState;
    }

    case "ADD_PAGE_ROTATION": {
      const { documentId, pageRotations } = action.payload;

      const newRotations: IPageRotation[] = pageRotations.map(
        (pageRotation, index) => ({
          ...pageRotation,
          id: String(`${+new Date()}-${index}`),
        })
      );
      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) => {
            if (item.documentId !== documentId) return item;
            const rotationExists = item.pageRotations.some(
              (rotation) => rotation.pageNumber === pageRotations[0].pageNumber
            );
            if (rotationExists) {
              return {
                ...item,
                pageRotations: item.pageRotations.map((rotation) =>
                  rotation.pageNumber === pageRotations[0].pageNumber
                    ? {
                        ...rotation,
                        rotationAngle: pageRotations[0].rotationAngle,
                      }
                    : rotation
                ),
              };
            }
            return {
              ...item,
              pageRotations: [...item.pageRotations, ...newRotations],
            };
          }),
        },
      };
      return newState;
    }

    case "REMOVE_PAGE_ROTATION": {
      const { rotationId, documentId } = action.payload;

      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  pageRotations: item.pageRotations.filter(
                    (rotation) => rotation.id !== rotationId
                  ),
                }
              : item
          ),
        },
      };

      return newState;
    }
    case "REMOVE_ALL_ROTATIONS": {
      const { documentId } = action.payload;
      const newState = {
        ...state,
        tabsState: {
          ...state.tabsState,
          items: state.tabsState.items.map((item) =>
            item.documentId === documentId
              ? {
                  ...item,
                  pageRotations: [],
                }
              : item
          ),
        },
      };

      return newState;
    }
    case "UPDATE_CONVERSION_STATUS": {
      const { documentId, status } = action.payload;
      const newState = {
        ...state,
        localDocumentState: {
          ...state.localDocumentState,
          [`${documentId}`]: {
            conversionStatus: status,
          },
        },
      };
      return newState;
    }
    case "ACCORDION_OPEN_CLOSE": {
      const { id, open } = action.payload;
      if (state.accordionState.status !== "succeeded") return state;
      const { sectionsOpenStatus } = state.accordionState.data;
      const nextSections = {
        ...sectionsOpenStatus,
        [id]: open,
      };
      return {
        ...state,
        accordionState: {
          ...state.accordionState,
          data: {
            ...state.accordionState.data,
            sectionsOpenStatus: nextSections,
            isAllOpen: Object.values(nextSections).every(
              (value) => value === true
            ),
          },
        },
      };
    }
    case "ACCORDION_OPEN_CLOSE_ALL": {
      if (state.accordionState.status !== "succeeded") return state;
      const { sectionsOpenStatus } = state.accordionState.data;

      const nextSections = Object.keys(sectionsOpenStatus).reduce(
        (accumulator, current) => {
          accumulator[current] = action.payload;
          return accumulator;
        },
        {} as any
      );
      return {
        ...state,
        accordionState: {
          ...state.accordionState,
          data: {
            ...state.accordionState.data,
            sectionsOpenStatus: nextSections,
            isAllOpen: action.payload,
          },
        },
      };
    }
    case "UPDATE_USED_UNUSED_DOCUMENT": {
      const { documentId, saveStatus, saveRefreshStatus } = action.payload;
      const newState = {
        ...state,
        usedOrUnused: {
          documentId: documentId,
          saveStatus: saveStatus,
          saveRefreshStatus: saveRefreshStatus,
        },
      };
      return newState;
    }
    case "DCF_DOCUMENT_VIEW_ACTION_CHANGE": {
      const { dcfMode: _dcfMode } = action.payload;
      const mode = {
        ...state,
        dcfMode: _dcfMode,
      };
      return mode;
    }

    default:
      throw new Error("Unknown action passed to case details reducer");
  }
};
