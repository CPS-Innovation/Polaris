import { useCallback, useEffect, useState, useMemo } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfViewer } from "../pdf-viewer/PdfViewer";
import { Wait } from "../pdf-viewer/Wait";
import { HeaderReadMode } from "./HeaderReadMode";
import { HeaderSearchMode } from "./HeaderSearchMode";
import { HeaderAttachmentMode } from "./HeaderAttachmentMode";
import { HeaderSearchPIIMode } from "./HeaderSearchPIIMode";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { SearchPIIRedactionWarningModal } from "../modals/SearchPIIRedactionWarningModal";
import { SearchPIIData } from "../../../domain/gateway/SearchPIIData";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { SaveRotationModal } from "../modals/SaveRotationModal";
import { LocalDocumentState } from "../../../domain/LocalDocumentState";
import {
  PageRotationDeletionWarningModal,
  RotationDeletionWarningModal,
} from "../modals/PageRotationDeletionWarningModal";
import { FeatureFlagData } from "../../../domain/FeatureFlagData";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { useAuthHeaderContext } from "../../../../../AuthHeaderProvider";
import classes from "./PdfTab.module.scss";

type PdfTabProps = {
  mappedDocument: MappedCaseDocument;
  caseId: number;
  redactionTypesData: RedactionTypeData[];
  tabIndex: number;
  activeTabId: string | undefined;
  tabId: string;
  showOverRedactionLog: boolean;
  featureFlags: FeatureFlagData;
  caseDocumentViewModel: CaseDocumentViewModel;
  headers: HeadersInit;
  searchPIIDataItem: SearchPIIData | undefined;
  savedDocumentDetails: {
    documentId: string;
    versionId: number;
  }[];
  localDocumentState: LocalDocumentState;
  handleOpenPdf: (caseDocument: {
    documentId: string;
    mode: "read" | "search";
  }) => void;
  handleLaunchSearchResults: () => void;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: CaseDetailsState["handleRemoveRedaction"];
  handleRemoveAllRedactions: CaseDetailsState["handleRemoveAllRedactions"];
  handleSavedRedactions: CaseDetailsState["handleSavedRedactions"];
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
  handleUpdateConversionStatus: CaseDetailsState["handleUpdateConversionStatus"];
  handleHideSaveRotationModal: CaseDetailsState["handleHideSaveRotationModal"];
};

export const PdfTab: React.FC<PdfTabProps> = ({
  mappedDocument,
  tabIndex,
  caseId,
  redactionTypesData,
  activeTabId,
  tabId,
  showOverRedactionLog,
  caseDocumentViewModel,
  headers,
  savedDocumentDetails,
  featureFlags,
  searchPIIDataItem,
  localDocumentState,
  handleOpenPdf,
  handleLaunchSearchResults,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
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
  handleUpdateConversionStatus,
  handleHideSaveRotationModal,
}) => {
  const { buildHeaders } = useAuthHeaderContext();
  const trackEvent = useAppInsightsTrackEvent();
  const [focussedHighlightIndex, setFocussedHighlightIndex] =
    useState<number>(0);

  const [showRedactionWarning, setShowRedactionWarning] = useState(false);
  const [showPageRotationDeletionWarning, setShowPageRotationDeletionWarning] =
    useState<{ show: boolean; type: RotationDeletionWarningModal } | null>(
      null
    );
  const [urlWithHeader, setUrlWithHeader] = useState<{
    url: string;
    headers: HeadersInit;
  } | null>(null);
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
    rotatePageMode,
    deletePageMode,
  } = caseDocumentViewModel;

  const {
    cmsDocType: { documentType = "" } = {},
    attachments,
    hasFailedAttachments,
    presentationFlags: { write: documentWriteStatus = "Ok" } = {},
    versionId,
  } = mappedDocument;
  const isDocumentRefreshing = useMemo(() => {
    return savedDocumentDetails.find(
      (document) => document.documentId === caseDocumentViewModel.documentId
    );
  }, [savedDocumentDetails, caseDocumentViewModel.documentId]);

  useEffect(() => {
    const updateNewUrlWithHeader = async (url: string) => {
      const headers = await buildHeaders();
      setUrlWithHeader({ url: url, headers: headers });
    };
    if (isDocumentRefreshing) {
      setUrlWithHeader(null);
      return;
    }
    if (url && url !== urlWithHeader?.url) {
      updateNewUrlWithHeader(url);
    }
  }, [url, urlWithHeader, buildHeaders, isDocumentRefreshing]);

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
    (documentId: string, showSuggestion: boolean, defaultOption: boolean) => {
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

  const documentUnAvailableReason = useMemo(() => {
    if (isDeleted) return "This document has been deleted and is unavailable.";
    if (
      localDocumentState[documentId]?.conversionStatus ===
      "EncryptionOrPasswordProtection"
    )
      return "This document has been encrypted or password protected and is unavailable.";
    if (
      localDocumentState[documentId]?.conversionStatus ===
      "UnsupportedFileTypeOrContent"
    )
      return "This document has unsupported file type or content and is unavailable.";

    return "This document is unavailable";
  }, [isDeleted, documentId, localDocumentState]);

  const isDocumentAvailable = useMemo(() => {
    return (
      !isDeleted &&
      (!localDocumentState[documentId]?.conversionStatus ||
        localDocumentState[documentId].conversionStatus === "DocumentConverted")
    );
  }, [isDeleted, documentId, localDocumentState]);

  const renderDocumentUnAvailable = () => {
    return (
      <div
        className={classes.unAvailableDocument}
        // data-testid={`deleted-document-notification-${documentId}`}
      >
        <p>{documentUnAvailableReason}</p>
      </div>
    );
  };

  const renderDocument = () => {
    return (
      <div>
        {mode === "search" ? (
          <HeaderSearchMode
            mappedDocument={mappedDocument}
            caseDocumentViewModel={caseDocumentViewModel}
            handleLaunchSearchResults={handleLaunchSearchResults}
            focussedHighlightIndex={focussedHighlightIndex}
            handleSetFocussedHighlightIndex={setFocussedHighlightIndex}
          />
        ) : (
          <HeaderReadMode
            showOverRedactionLog={showOverRedactionLog}
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
            mappedDocument={mappedDocument}
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

        {urlWithHeader?.url ? (
          <PdfViewer
            key={`${documentId}-${versionId}-${urlWithHeader?.url ?? ""}`}
            redactionTypesData={redactionTypesData}
            url={urlWithHeader?.url}
            tabIndex={tabIndex}
            activeTabId={activeTabId}
            tabId={tabId}
            headers={urlWithHeader?.headers}
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
            handleUpdateConversionStatus={handleUpdateConversionStatus}
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
      </div>
    );
  };

  return (
    <div>
      <div aria-live="polite" className={classes.visuallyHidden}>
        {isDocumentAvailable ? "" : documentUnAvailableReason}
      </div>
      {isDocumentAvailable ? renderDocument() : renderDocumentUnAvailable()}
    </div>
  );
};
