import { AsyncPipelineResult } from "../hooks/use-pipeline-api/AsyncPipelineResult";
import { AsyncResult } from "../../../common/types/AsyncResult";
import { CaseDetails } from "./gateway/CaseDetails";

import { CaseDocumentViewModel } from "./CaseDocumentViewModel";
import { PipelineResults } from "./gateway/PipelineResults";
import { MappedTextSearchResult } from "./MappedTextSearchResult";
import { AccordionDocumentSection } from "../presentation/case-details/accordion/types";
import { MappedCaseDocument } from "./MappedCaseDocument";
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

export type CombinedState = {
  urn: string;
  caseId: number;
  caseState: AsyncResult<CaseDetails>;
  documentsState: AsyncResult<MappedCaseDocument[]>;
  pipelineState: AsyncPipelineResult<PipelineResults>;
  pipelineRefreshData: {
    startRefresh: boolean;
    savedDocumentDetails: {
      documentId: string;
      polarisDocumentVersionId: number;
    }[];
    lastProcessingCompleted: string;
  };
  accordionState: AsyncResult<AccordionDocumentSection[]>;
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
    requestedSearchTerm: undefined | string;
    submittedSearchTerm: undefined | string;
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
};
