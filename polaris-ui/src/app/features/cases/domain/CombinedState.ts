import { AsyncPipelineResult } from "../hooks/use-pipeline-api/AsyncPipelineResult";
import { AsyncResult } from "../../../common/types/AsyncResult";
import { CaseDetails } from "./gateway/CaseDetails";
import { CaseDocumentViewModel } from "./CaseDocumentViewModel";
import { PipelineResults } from "./gateway/PipelineResults";
import { MappedTextSearchResult } from "./MappedTextSearchResult";
import { AccordionData } from "../presentation/case-details/accordion/types";
import { MappedCaseDocument } from "./MappedCaseDocument";
import { LocalDocumentState } from "./LocalDocumentState";
import { FeatureFlagData } from "./FeatureFlagData";
import { FilterOption } from "./FilterOption";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
  RedactionTypeData,
} from "./redactionLog/RedactionLogData";
import { RedactionLogTypes } from "../domain/redactionLog/RedactionLogTypes";
import { StoredUserData } from "./gateway/StoredUserData";
import { ErrorModalTypes } from "./ErrorModalTypes";
import { NotesData } from "../domain/gateway/NotesData";
import { SearchPIIData } from "./gateway/SearchPIIData";
import { RenameDocumentData } from "./gateway/RenameDocumentData";
import { ReclassifyDocumentData } from "./gateway/ReclassifyDocumentData";
import { TaggedContext } from "../../../inbound-handover/context";
import {
  buildDefaultNotificationState,
  NotificationState,
} from "./NotificationState";

export type CombinedState = {
  urn: string;
  caseId: number;
  context: TaggedContext | undefined;
  caseState: AsyncResult<CaseDetails>;
  documentsState: AsyncResult<MappedCaseDocument[]>;
  pipelineState: AsyncPipelineResult<PipelineResults>;
  localDocumentState: LocalDocumentState;
  documentRefreshData: {
    startDocumentRefresh: boolean;
    savedDocumentDetails: {
      documentId: string;
      versionId: number;
    }[];
  };
  pipelineRefreshData: {
    startPipelineRefresh: boolean;
    lastProcessingCompleted: string;
    localLastRefreshTime: string;
  };
  accordionState: AsyncResult<AccordionData>;
  notificationState: NotificationState;
  tabsState: {
    items: CaseDocumentViewModel[];
    headers: HeadersInit;
    activeTabId: string | undefined;
  };
  // `searchTerm` is outside of `searchState` as it is more volatile: when
  //  the user is typing away, we prevent `searchState` from continually being
  //  rebuilt and avoiding rerenders caused by continual props reference changes.
  searchTerm: string;
  searchState: {
    isResultsVisible: boolean;
    requestedSearchTerm: string | undefined;
    submittedSearchTerm: string | undefined;
    lastSubmittedSearchTerm: string | undefined;
    resultsOrder: "byDateDesc" | "byOccurancesPerDocumentDesc";
    filterOptions: {
      docType: { [key: string]: FilterOption };
      category: { [key: string]: FilterOption };
    };
    missingDocs: {
      documentId: CaseDocumentViewModel["documentId"];
      fileName: string;
    }[];
    results: AsyncResult<MappedTextSearchResult>;
  };
  errorModal: {
    show: boolean;
    message: string;
    title: string;
    type: ErrorModalTypes;
  };
  documentIssueModal: {
    show: boolean;
  };
  redactionLog: {
    showModal: boolean;
    type: RedactionLogTypes;
    redactionLogLookUpsData: AsyncResult<RedactionLogLookUpsData>;
    redactionLogMappingData: AsyncResult<RedactionLogMappingData>;
    savedRedactionTypes: RedactionTypeData[];
  };
  featureFlags: FeatureFlagData;
  storedUserData: AsyncResult<StoredUserData>;
  notes: NotesData[];
  searchPII: SearchPIIData[];
  renameDocuments: RenameDocumentData[];
  reclassifyDocuments: ReclassifyDocumentData[];
};

export const initialState = {
  caseState: { status: "loading" },
  documentsState: { status: "loading" },
  pipelineState: { status: "initiating", correlationId: "" },
  localDocumentState: {},
  documentRefreshData: {
    startDocumentRefresh: true,
    savedDocumentDetails: [],
  },
  pipelineRefreshData: {
    startPipelineRefresh: false,
    lastProcessingCompleted: "",
    localLastRefreshTime: "",
  },
  accordionState: { status: "loading" },
  tabsState: { items: [], headers: {}, activeTabId: undefined },
  notificationState: buildDefaultNotificationState(),
  searchTerm: "",
  searchState: {
    isResultsVisible: false,
    requestedSearchTerm: undefined,
    submittedSearchTerm: undefined,
    lastSubmittedSearchTerm: undefined,
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
    renameDocument: false,
    reclassify: false,
    externalRedirectCaseReviewApp: false,
    externalRedirectBulkUmApp: false,
    pageDelete: false,
    pageRotate: false,
    notifications: false,
    stateRetention: false,
    globalNav: false,
    copyRedactionTextButton: false,
    documentNameSearch: false,
  },
  storedUserData: { status: "loading" },
  notes: [],
  searchPII: [],
  renameDocuments: [],
  reclassifyDocuments: [],
  context: undefined,
} as Omit<CombinedState, "caseId" | "urn">;
