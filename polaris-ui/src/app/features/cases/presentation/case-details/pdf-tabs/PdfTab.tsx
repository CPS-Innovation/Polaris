import { useCallback } from "react";
import { useState } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { NewPdfHighlight } from "../../../domain/NewPdfHighlight";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfViewer } from "../pdf-viewer/PdfViewer";
import { Wait } from "../pdf-viewer/Wait";
import { HeaderReadMode } from "./HeaderReadMode";
import { HeaderSearchMode } from "./HeaderSearchMode";
import { PresentationFlags } from "../../../../../features/cases/domain/PipelineDocument";

type PdfTabProps = {
  tabIndex: number;
  caseDocumentViewModel: CaseDocumentViewModel;
  headers: HeadersInit;
  redactStatus: PresentationFlags["write"];
  savedDocumentDetails: {
    documentId: string;
    polarisDocumentVersionId: number;
  }[];
  handleLaunchSearchResults: () => void;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: CaseDetailsState["handleRemoveRedaction"];
  handleRemoveAllRedactions: CaseDetailsState["handleRemoveAllRedactions"];
  handleSavedRedactions: CaseDetailsState["handleSavedRedactions"];
  handleOpenPdfInNewTab: CaseDetailsState["handleOpenPdfInNewTab"];
};

export const PdfTab: React.FC<PdfTabProps> = ({
  tabIndex,
  caseDocumentViewModel,
  headers,
  redactStatus,
  savedDocumentDetails,
  handleLaunchSearchResults,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  handleOpenPdfInNewTab,
}) => {
  const [focussedHighlightIndex, setFocussedHighlightIndex] =
    useState<number>(0);

  const { url, mode, redactionHighlights, documentId } = caseDocumentViewModel;

  const searchHighlights =
    mode === "search" ? caseDocumentViewModel.searchHighlights : undefined;

  const localHandleAddRedaction = useCallback(
    (redaction: NewPdfHighlight) => handleAddRedaction(documentId, redaction),
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
          caseDocumentViewModel={caseDocumentViewModel}
          handleOpenPdfInNewTab={handleOpenPdfInNewTab}
        />
      )}

      {url && !isDocumentRefreshing() ? (
        <PdfViewer
          url={url}
          tabIndex={tabIndex}
          headers={headers}
          searchHighlights={searchHighlights}
          redactStatus={redactStatus}
          redactionHighlights={redactionHighlights}
          focussedHighlightIndex={focussedHighlightIndex}
          handleAddRedaction={localHandleAddRedaction}
          handleRemoveRedaction={localHandleRemoveRedaction}
          handleRemoveAllRedactions={localHandleRemoveAllRedactions}
          handleSavedRedactions={localHandleSavedRedactions}
        />
      ) : (
        <Wait dataTestId={`pdfTab-spinner-${tabIndex}`} />
      )}
    </>
  );
};
