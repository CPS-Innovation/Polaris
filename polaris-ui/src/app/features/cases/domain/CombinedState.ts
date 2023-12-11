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
  RedactionLogData,
  RedactionTypes,
} from "./redactionLog/RedactionLogData";

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
  };
  documentIssueModal: {
    show: boolean;
  };
  redactionLog: {
    showModal: boolean;
    redactionLogData: AsyncResult<RedactionLogData>;
    redactionTypes: RedactionTypes[];
  };
  featureFlags: AsyncResult<FeatureFlagData>;
};
