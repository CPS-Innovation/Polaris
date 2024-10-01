import { useCallback } from "react";
import { useState, useMemo } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfViewer } from "../pdf-viewer/PdfViewer";
import { Wait } from "../pdf-viewer/Wait";
import { HeaderReadMode } from "./HeaderReadMode";
import { HeaderSearchMode } from "./HeaderSearchMode";
import { HeaderAttachmentMode } from "./HeaderAttachmentMode";
import { HeaderSearchPIIMode } from "./HeaderSearchPIIMode";
import { PresentationFlags } from "../../../domain/gateway/PipelineDocument";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { SearchPIIRedactionWarningModal } from "../modals/SearchPIIRedactionWarningModal";
import { SearchPIIData } from "../../../domain/gateway/SearchPIIData";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./PdfTab.module.scss";
type PdfTabProps = {
  caseId: number;
  redactionTypesData: RedactionTypeData[];
  tabIndex: number;
  activeTabId: string | undefined;
  tabId: string;
  showOverRedactionLog: boolean;
  caseDocumentViewModel: CaseDocumentViewModel;
  headers: HeadersInit;
  documentWriteStatus: PresentationFlags["write"];
  searchPIIDataItem: SearchPIIData | undefined;
  polarisDocumentVersionId: number;
  savedDocumentDetails: {
    documentId: string;
    polarisDocumentVersionId: number;
  }[];
  contextData: {
    correlationId: string;
    showSearchPII: boolean;
    showDeletePage: boolean;
  };
  isOkToSave: boolean;
  handleOpenPdf: (caseDocument: {
    documentId: string;
    mode: "read" | "search";
  }) => void;
  handleLaunchSearchResults: () => void;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: CaseDetailsState["handleRemoveRedaction"];
  handleRemoveAllRedactions: CaseDetailsState["handleRemoveAllRedactions"];
  handleSavedRedactions: CaseDetailsState["handleSavedRedactions"];
  handleShowHideDocumentIssueModal: CaseDetailsState["handleShowHideDocumentIssueModal"];
  handleShowRedactionLogModal: CaseDetailsState["handleShowRedactionLogModal"];
  handleAreaOnlyRedaction: CaseDetailsState["handleAreaOnlyRedaction"];
  handleShowHideRedactionSuggestions: CaseDetailsState["handleShowHideRedactionSuggestions"];
  handleSearchPIIAction: CaseDetailsState["handleSearchPIIAction"];
  handleShowHidePageRotation: CaseDetailsState["handleShowHidePageRotation"];
  handleAddPageRotation: CaseDetailsState["handleAddPageRotation"];
  handleRemovePageRotation: CaseDetailsState["handleRemovePageRotation"];
};

export const PdfTab: React.FC<PdfTabProps> = ({
  tabIndex,
  caseId,
  redactionTypesData,
  activeTabId,
  tabId,
  polarisDocumentVersionId,
  showOverRedactionLog,
  caseDocumentViewModel,
  headers,
  documentWriteStatus,
  savedDocumentDetails,
  contextData,
  isOkToSave,
  searchPIIDataItem,
  handleOpenPdf,
  handleLaunchSearchResults,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  handleShowHideDocumentIssueModal,
  handleShowRedactionLogModal,
  handleAreaOnlyRedaction,
  handleShowHideRedactionSuggestions,
  handleSearchPIIAction,
  handleShowHidePageRotation,
  handleAddPageRotation,
  handleRemovePageRotation,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [focussedHighlightIndex, setFocussedHighlightIndex] =
    useState<number>(0);

  const [showRedactionWarning, setShowRedactionWarning] = useState(false);
  const {
    url,
    mode,
    redactionHighlights,
    pageDeleteRedactions,
    pageRotations,
    documentId,
    areaOnlyRedactionMode,
    isDeleted,
    saveStatus,
    cmsDocType: { documentType },
    attachments,
    hasFailedAttachments,
    rotatePageMode,
  } = caseDocumentViewModel;

  const searchHighlights =
    mode === "search" ? caseDocumentViewModel.searchHighlights : undefined;

  const activeSearchPIIHighlights = useMemo(() => {
    if (!searchPIIDataItem?.show) {
      return [];
    }
    return (
      searchPIIDataItem?.searchPIIHighlights.filter(
        (highlight) => highlight.redactionStatus === "initial"
      ) ?? []
    );
  }, [searchPIIDataItem]);

  const localHandleRemoveRedaction = useCallback(
    (redactionId: string) => handleRemoveRedaction(documentId, redactionId),
    [documentId, handleRemoveRedaction]
  );

  const localHandleRemoveAllRedactions = useCallback(() => {
    handleRemoveAllRedactions(documentId);

    const piiGroupIds = redactionHighlights.reduce((acc, highlight) => {
      if (highlight?.searchPIIId) {
        acc.push(highlight?.searchPIIId);
      }
      return acc;
    }, [] as string[]);

    if (piiGroupIds.length) {
      handleSearchPIIAction(documentId, "initial", piiGroupIds);
    }
  }, [
    documentId,
    handleRemoveAllRedactions,
    handleSearchPIIAction,
    redactionHighlights,
  ]);

  const localHandleShowHideRedactionSuggestions = useCallback(
    (documentId, showSuggestion, defaultOption) => {
      const getData =
        searchPIIDataItem?.getSearchPIIStatus === "failure"
          ? true
          : searchPIIDataItem?.polarisDocumentVersionId !==
            polarisDocumentVersionId;
      handleShowHideRedactionSuggestions(
        documentId,
        showSuggestion,
        getData,
        defaultOption
      );
    },
    [
      handleShowHideRedactionSuggestions,
      searchPIIDataItem,
      polarisDocumentVersionId,
    ]
  );

  const saveAllRedactionsCustomEvent = () => {
    trackEvent("Save All Redactions", {
      documentType: documentType,
      documentId: documentId,
      redactionsCount:
        redactionHighlights?.length + pageDeleteRedactions?.length,
      deletedPageCount: pageDeleteRedactions?.length,
      suggestedRedactionsCount: searchPIIDataItem?.searchPIIHighlights?.length,
      acceptedSuggestedRedactionsCount: activeSearchPIIHighlights?.length,
    });
  };

  const localHandleSavedRedactions = () => {
    if (acceptedAllSearchPIIRedactionsCount) {
      setShowRedactionWarning(true);
      return;
    }
    handleSavedRedactions(documentId, isSearchPIIOn);
    saveAllRedactionsCustomEvent();
  };

  const isDocumentRefreshing = () => {
    return savedDocumentDetails.find(
      (document) => document.documentId === caseDocumentViewModel.documentId
    );
  };
  const isSearchPIIOn = useMemo(() => {
    return !!searchPIIDataItem?.show;
  }, [searchPIIDataItem]);

  const acceptedAllSearchPIIRedactionsCount = useMemo(() => {
    const acceptedAllRedactions =
      searchPIIDataItem?.searchPIIHighlights.filter(
        (highlight) => highlight.redactionStatus === "acceptedAll"
      ) ?? [];
    return acceptedAllRedactions.length;
  }, [searchPIIDataItem?.searchPIIHighlights]);

  const handleContinue = () => {
    setShowRedactionWarning(false);
    handleSavedRedactions(documentId, isSearchPIIOn);
    saveAllRedactionsCustomEvent();
  };

  if (isDeleted) {
    return (
      <div
        className={classes.deletedDocument}
        data-testid={`deleted-document-notification-${documentId}`}
      >
        <p>This document has been deleted and is unavailable.</p>
      </div>
    );
  }

  return (
    <>
      {mode === "search" ? (
        <HeaderSearchMode
          caseDocumentViewModel={caseDocumentViewModel}
          handleLaunchSearchResults={handleLaunchSearchResults}
          focussedHighlightIndex={focussedHighlightIndex}
          handleSetFocussedHighlightIndex={setFocussedHighlightIndex}
        />
      ) : (
        <HeaderReadMode
          showOverRedactionLog={showOverRedactionLog}
          caseDocumentViewModel={caseDocumentViewModel}
          handleShowHideDocumentIssueModal={handleShowHideDocumentIssueModal}
          handleShowRedactionLogModal={handleShowRedactionLogModal}
          handleAreaOnlyRedaction={handleAreaOnlyRedaction}
          handleShowHideRedactionSuggestions={
            localHandleShowHideRedactionSuggestions
          }
          handleShowHidePageRotation={handleShowHidePageRotation}
          contextData={{
            documentId: documentId,
            tabIndex: tabIndex,
            areaOnlyRedactionMode: areaOnlyRedactionMode,
            isSearchPIIOn: isSearchPIIOn,
            isSearchPIIDefaultOptionOn: !!searchPIIDataItem?.defaultOption,
            showSearchPII: contextData.showSearchPII,
            isRotatePageModeOn: rotatePageMode,
          }}
        />
      )}
      {!!attachments.length && (
        <HeaderAttachmentMode
          caseDocumentViewModel={caseDocumentViewModel}
          handleOpenPdf={handleOpenPdf}
        />
      )}
      {isSearchPIIOn && (
        <HeaderSearchPIIMode
          activeSearchPIIHighlights={activeSearchPIIHighlights}
          getSearchPIIStatus={searchPIIDataItem?.getSearchPIIStatus}
        />
      )}
      {hasFailedAttachments && (
        <div className={classes.attachmentHeaderContent}>
          <span
            className={classes.failedAttachmentWarning}
            data-testid={`failed-attachment-warning-${documentId}`}
          >
            Attachments only available on CMS
          </span>
        </div>
      )}

      {url && !isDocumentRefreshing() ? (
        <PdfViewer
          redactionTypesData={redactionTypesData}
          url={url}
          tabIndex={tabIndex}
          activeTabId={activeTabId}
          tabId={tabId}
          headers={headers}
          searchHighlights={searchHighlights}
          isSearchPIIOn={isSearchPIIOn}
          isSearchPIIDefaultOptionOn={!!searchPIIDataItem?.defaultOption}
          activeSearchPIIHighlights={activeSearchPIIHighlights}
          documentWriteStatus={documentWriteStatus}
          contextData={{
            documentId,
            documentType,
            saveStatus: saveStatus,
            caseId,
            showDeletePage: contextData.showDeletePage,
          }}
          isOkToSave={isOkToSave}
          redactionHighlights={redactionHighlights}
          pageDeleteRedactions={pageDeleteRedactions}
          pageRotations={pageRotations}
          focussedHighlightIndex={focussedHighlightIndex}
          areaOnlyRedactionMode={areaOnlyRedactionMode}
          rotatePageMode={rotatePageMode}
          handleAddRedaction={handleAddRedaction}
          handleRemoveRedaction={localHandleRemoveRedaction}
          handleAddPageRotation={handleAddPageRotation}
          handleRemovePageRotation={handleRemovePageRotation}
          handleRemoveAllRedactions={localHandleRemoveAllRedactions}
          handleSavedRedactions={localHandleSavedRedactions}
          handleSearchPIIAction={handleSearchPIIAction}
        />
      ) : (
        <Wait
          dataTestId={`pdfTab-spinner-${tabIndex}`}
          ariaLabel="Refreshing document, please wait"
        />
      )}

      {showRedactionWarning && (
        <SearchPIIRedactionWarningModal
          documentId={documentId}
          documentType={documentType}
          acceptedAllSearchPIIRedactionsCount={
            acceptedAllSearchPIIRedactionsCount
          }
          handleContinue={handleContinue}
          polarisDocumentVersionId={polarisDocumentVersionId!}
          hideRedactionWarningModal={() => setShowRedactionWarning(false)}
        />
      )}
    </>
  );
};
