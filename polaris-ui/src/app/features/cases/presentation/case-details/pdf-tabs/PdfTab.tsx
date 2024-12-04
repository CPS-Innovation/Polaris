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
import { SaveRotationModal } from "../modals/SaveRotationModal";
import {
  PageRotationDeletionWarningModal,
  RotationDeletionWarningModal,
} from "../modals/PageRotationDeletionWarningModal";
import { FeatureFlagData } from "../../../domain/FeatureFlagData";
import classes from "./PdfTab.module.scss";

type PdfTabProps = {
  caseId: number;
  redactionTypesData: RedactionTypeData[];
  tabIndex: number;
  activeTabId: string | undefined;
  tabId: string;
  showOverRedactionLog: boolean;
  featureFlags: FeatureFlagData;
  caseDocumentViewModel: CaseDocumentViewModel;
  headers: HeadersInit;
  documentWriteStatus: PresentationFlags["write"];
  searchPIIDataItem: SearchPIIData | undefined;
  versionId: number;
  savedDocumentDetails: {
    documentId: string;
    versionId: number;
  }[];
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
  handleShowHidePageDeletion: CaseDetailsState["handleShowHidePageDeletion"];
  handleAddPageRotation: CaseDetailsState["handleAddPageRotation"];
  handleRemovePageRotation: CaseDetailsState["handleRemovePageRotation"];
  handleRemoveAllRotations: CaseDetailsState["handleRemoveAllRotations"];
  handleSaveRotations: CaseDetailsState["handleSaveRotations"];
  handleHideSaveRotationModal: CaseDetailsState["handleHideSaveRotationModal"];
};

export const PdfTab: React.FC<PdfTabProps> = ({
  tabIndex,
  caseId,
  redactionTypesData,
  activeTabId,
  tabId,
  versionId,
  showOverRedactionLog,
  caseDocumentViewModel,
  headers,
  documentWriteStatus,
  savedDocumentDetails,
  featureFlags,
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
  handleShowHidePageDeletion,
  handleAddPageRotation,
  handleRemovePageRotation,
  handleRemoveAllRotations,
  handleSaveRotations,
  handleHideSaveRotationModal,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [focussedHighlightIndex, setFocussedHighlightIndex] =
    useState<number>(0);

  const [showRedactionWarning, setShowRedactionWarning] = useState(false);
  const [showPageRotationDeletionWarning, setShowPageRotationDeletionWarning] =
    useState<{ show: boolean; type: RotationDeletionWarningModal } | null>(
      null
    );
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
    deletePageMode,
  } = caseDocumentViewModel;

  const showDeletePage = useMemo(
    () =>
      featureFlags.pageDelete &&
      documentType !== "DAC" &&
      documentType !== "PCD",
    [featureFlags.pageDelete, documentType]
  );

  const showRotatePage = useMemo(
    () =>
      featureFlags.pageRotate &&
      documentType !== "DAC" &&
      documentType !== "PCD",
    [featureFlags.pageRotate, documentType]
  );

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
          : searchPIIDataItem?.versionId !== versionId;
      handleShowHideRedactionSuggestions(
        documentId,
        versionId,
        showSuggestion,
        getData,
        defaultOption
      );
    },
    [handleShowHideRedactionSuggestions, searchPIIDataItem, versionId]
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

  const localHandleShowHidePageRotation = (
    documentId: string,
    newRotatePageMode: boolean
  ) => {
    if (
      newRotatePageMode &&
      redactionHighlights?.length + pageDeleteRedactions?.length
    ) {
      setShowPageRotationDeletionWarning({
        show: true,
        type: "ShowRotationWarning",
      });
      return;
    }
    if (!newRotatePageMode) {
      const unSavedRotations = pageRotations.filter(
        (rotation) => rotation.rotationAngle !== 0
      );
      if (unSavedRotations?.length) {
        setShowPageRotationDeletionWarning({
          show: true,
          type: "HideRotationWarning",
        });
        return;
      }
    }

    handleShowHidePageRotation(documentId, newRotatePageMode);
  };

  const localHandleShowHidePageDeletion = (
    documentId: string,
    newDeletePageMode: boolean
  ) => {
    if (newDeletePageMode) {
      const unSavedRotations = pageRotations.filter(
        (rotation) => rotation.rotationAngle !== 0
      );
      if (unSavedRotations?.length) {
        setShowPageRotationDeletionWarning({
          show: true,
          type: "ShowDeletionWarning",
        });
        return;
      }
    }
    if (!newDeletePageMode && pageDeleteRedactions?.length) {
      setShowPageRotationDeletionWarning({
        show: true,
        type: "HideDeletionWarning",
      });
      return;
    }

    handleShowHidePageDeletion(documentId, newDeletePageMode);
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
          handleShowHidePageRotation={localHandleShowHidePageRotation}
          handleShowHidePageDeletion={localHandleShowHidePageDeletion}
          contextData={{
            documentId: documentId,
            tabIndex: tabIndex,
            areaOnlyRedactionMode: areaOnlyRedactionMode,
            isSearchPIIOn: isSearchPIIOn,
            isSearchPIIDefaultOptionOn: !!searchPIIDataItem?.defaultOption,
            showSearchPII: featureFlags.searchPII,
            rotatePageMode: rotatePageMode,
            deletePageMode: deletePageMode,
            showRotatePage: showRotatePage,
            showDeletePage: showDeletePage,
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
            showDeletePage: showDeletePage && deletePageMode,
            showRotatePage: showRotatePage && rotatePageMode,
          }}
          isOkToSave={isOkToSave}
          redactionHighlights={redactionHighlights}
          pageDeleteRedactions={pageDeleteRedactions}
          pageRotations={pageRotations}
          focussedHighlightIndex={focussedHighlightIndex}
          areaOnlyRedactionMode={areaOnlyRedactionMode}
          handleAddRedaction={handleAddRedaction}
          handleRemoveRedaction={localHandleRemoveRedaction}
          handleAddPageRotation={handleAddPageRotation}
          handleRemovePageRotation={handleRemovePageRotation}
          handleRemoveAllRedactions={localHandleRemoveAllRedactions}
          handleSavedRedactions={localHandleSavedRedactions}
          handleSearchPIIAction={handleSearchPIIAction}
          handleRemoveAllRotations={handleRemoveAllRotations}
          handleSaveRotations={handleSaveRotations}
        />
      ) : (
        <Wait
          dataTestId={`pdfTab-spinner-${tabIndex}`}
          ariaLabel="Refreshing document, please wait"
        />
      )}
      <SaveRotationModal
        saveStatus={saveStatus}
        handleCloseSaveRotationModal={() =>
          handleHideSaveRotationModal(documentId)
        }
      />

      {showRedactionWarning && (
        <SearchPIIRedactionWarningModal
          documentId={documentId}
          documentType={documentType}
          acceptedAllSearchPIIRedactionsCount={
            acceptedAllSearchPIIRedactionsCount
          }
          handleContinue={handleContinue}
          versionId={versionId!}
          hideRedactionWarningModal={() => setShowRedactionWarning(false)}
        />
      )}
      {showPageRotationDeletionWarning?.show && (
        <PageRotationDeletionWarningModal
          type={showPageRotationDeletionWarning?.type}
          hidePageRotationDeletionWarningModal={() =>
            setShowPageRotationDeletionWarning(null)
          }
        />
      )}
    </>
  );
};
