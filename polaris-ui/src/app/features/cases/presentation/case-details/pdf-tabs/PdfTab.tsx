import { useCallback } from "react";
import { useState, useMemo } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { NewPdfHighlight } from "../../../domain/NewPdfHighlight";
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
    searchPIIOn: string[];
    showSearchPII: boolean;
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
  handleIgnoreRedactionSuggestion: CaseDetailsState["handleIgnoreRedactionSuggestion"];
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
  handleIgnoreRedactionSuggestion,
}) => {
  const [focussedHighlightIndex, setFocussedHighlightIndex] =
    useState<number>(0);

  const [showRedactionWarning, setShowRedactionWarning] = useState(false);
  const {
    url,
    mode,
    redactionHighlights,
    documentId,
    areaOnlyRedactionMode,
    isDeleted,
    saveStatus,
    cmsDocType: { documentType },
    attachments,
    hasFailedAttachments,
  } = caseDocumentViewModel;

  const searchHighlights =
    mode === "search" ? caseDocumentViewModel.searchHighlights : undefined;

  const searchPIIHighlights = useMemo(() => {
    if (!searchPIIDataItem?.show) {
      return [];
    }
    return (
      searchPIIDataItem?.searchPIIHighlights.filter(
        (highlight) => highlight.redactionStatus === "redacted"
      ) ?? []
    );
  }, [searchPIIDataItem]);

  const localHandleAddRedaction = useCallback(
    (redactions: NewPdfHighlight[]) =>
      handleAddRedaction(documentId, redactions),
    [documentId, handleAddRedaction]
  );

  const localHandleRemoveRedaction = useCallback(
    (redactionId: string) => handleRemoveRedaction(documentId, redactionId),
    [documentId, handleRemoveRedaction]
  );

  const localHandleRemoveAllRedactions = useCallback(() => {
    handleRemoveAllRedactions(documentId);
    handleShowHideRedactionSuggestions(documentId, false, false);
  }, [
    documentId,
    handleRemoveAllRedactions,
    handleShowHideRedactionSuggestions,
  ]);

  const localHandleShowHideRedactionSuggestions = useCallback(
    (documentId, showSuggestion) => {
      handleShowHideRedactionSuggestions(
        documentId,
        showSuggestion,
        searchPIIDataItem?.polarisDocumentVersionId !== polarisDocumentVersionId
      );
    },
    [
      handleShowHideRedactionSuggestions,
      searchPIIDataItem,
      polarisDocumentVersionId,
    ]
  );

  const localHandleSavedRedactions = () => {
    if (isSearchPIIOn) {
      setShowRedactionWarning(true);
      return;
    }
    handleSavedRedactions(documentId);
  };

  const isDocumentRefreshing = () => {
    return savedDocumentDetails.find(
      (document) => document.documentId === caseDocumentViewModel.documentId
    );
  };
  const isSearchPIIOn = useMemo(() => {
    return contextData.searchPIIOn.includes(documentId);
  }, [contextData.searchPIIOn, documentId]);

  const handleContinue = () => {
    setShowRedactionWarning(false);
    handleSavedRedactions(documentId, true);
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
          contextData={{
            documentId: documentId,
            tabIndex: tabIndex,
            areaOnlyRedactionMode: areaOnlyRedactionMode,
            isSearchPIIOn: isSearchPIIOn,
            showSearchPII: contextData.showSearchPII,
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
        <HeaderSearchPIIMode searchPIIHighlights={searchPIIHighlights} />
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
          searchPIIHighlights={searchPIIHighlights}
          searchPIIGroupedText={
            searchPIIDataItem?.show
              ? searchPIIDataItem?.groupedTextByGroupId ?? {}
              : {}
          }
          documentWriteStatus={documentWriteStatus}
          contextData={{
            documentId,
            documentType,
            saveStatus: saveStatus,
            caseId,
          }}
          isOkToSave={isOkToSave}
          redactionHighlights={redactionHighlights}
          focussedHighlightIndex={focussedHighlightIndex}
          areaOnlyRedactionMode={areaOnlyRedactionMode}
          handleAddRedaction={localHandleAddRedaction}
          handleRemoveRedaction={localHandleRemoveRedaction}
          handleRemoveAllRedactions={localHandleRemoveAllRedactions}
          handleSavedRedactions={localHandleSavedRedactions}
          handleIgnoreRedactionSuggestion={handleIgnoreRedactionSuggestion}
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
          searchPIIHighlights={searchPIIHighlights}
          handleContinue={handleContinue}
          // presentationTitle={contextData.presentationTitle!}
          // polarisDocumentVersionId={polarisDocumentVersionId!}

          hideRedactionWarningModal={() => setShowRedactionWarning(false)}
        />
      )}
    </>
  );
};
