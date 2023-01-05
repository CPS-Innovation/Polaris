import { useCallback } from "react";
import { useState } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { NewPdfHighlight } from "../../../domain/NewPdfHighlight";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfViewer } from "../pdf-viewer/PdfViewer";
import { Wait } from "../pdf-viewer/Wait";
import { HeaderReadMode } from "./HeaderReadMode";
import { HeaderSearchMode } from "./HeaderSearchMode";

type PdfTabProps = {
  caseDocumentViewModel: CaseDocumentViewModel;
  headers: HeadersInit;
  handleLaunchSearchResults: () => void;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: CaseDetailsState["handleRemoveRedaction"];
  handleRemoveAllRedactions: CaseDetailsState["handleRemoveAllRedactions"];
  handleSavedRedactions: CaseDetailsState["handleSavedRedactions"];
  handleOpenPdfInNewTab: CaseDetailsState["handleOpenPdfInNewTab"];
};

export const PdfTab: React.FC<PdfTabProps> = ({
  caseDocumentViewModel,
  headers,
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

      {url ? (
        <PdfViewer
          url={url}
          headers={headers}
          searchHighlights={searchHighlights}
          redactionHighlights={redactionHighlights}
          focussedHighlightIndex={focussedHighlightIndex}
          handleAddRedaction={localHandleAddRedaction}
          handleRemoveRedaction={localHandleRemoveRedaction}
          handleRemoveAllRedactions={localHandleRemoveAllRedactions}
          handleSavedRedactions={localHandleSavedRedactions}
        />
      ) : (
        <Wait />
      )}
    </>
  );
};
