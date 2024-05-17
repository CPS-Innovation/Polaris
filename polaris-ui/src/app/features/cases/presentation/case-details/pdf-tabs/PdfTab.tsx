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
import { ISearchPIIHighlight } from "../../../domain/NewPdfHighlight";
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
  searchPIIHighlights: ISearchPIIHighlight[];
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
  showOverRedactionLog,
  caseDocumentViewModel,
  headers,
  documentWriteStatus,
  savedDocumentDetails,
  contextData,
  isOkToSave,
  searchPIIHighlights,
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

  const localHandleAddRedaction = useCallback(
    (redactions: NewPdfHighlight[]) =>
      handleAddRedaction(documentId, redactions),
    [documentId, handleAddRedaction]
  );

  const localHandleRemoveRedaction = useCallback(
    (redactionId: string) => handleRemoveRedaction(documentId, redactionId),
    [documentId, handleRemoveRedaction]
  );

  const localHandleRemoveAllRedactions = useCallback(
    () => handleRemoveAllRedactions(documentId),
    [documentId, handleRemoveAllRedactions]
  );

  const localHandleSavedRedactions = useCallback(
    () => handleSavedRedactions(documentId),
    [documentId, handleSavedRedactions]
  );

  const isDocumentRefreshing = () => {
    return savedDocumentDetails.find(
      (document) => document.documentId === caseDocumentViewModel.documentId
    );
  };
  const isSearchPIIOn = useMemo(() => {
    return contextData.searchPIIOn.includes(documentId);
  }, [contextData.searchPIIOn, documentId]);

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
            handleShowHideRedactionSuggestions
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
      {isSearchPIIOn && <HeaderSearchPIIMode />}
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
    </>
  );
};
