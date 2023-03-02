import { Tabs } from "../../../../../common/presentation/components/tabs";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { getRedactStatus } from "../utils/pdfTabsUtils";
import { PdfTab } from "./PdfTab";

type PdfTabsProps = {
  tabsState: {
    items: CaseDocumentViewModel[];
    headers: HeadersInit;
  };
  pipelineState: CaseDetailsState["pipelineState"];
  handleClosePdf: (caseDocument: { tabSafeId: string }) => void;
  handleLaunchSearchResults: () => void;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: CaseDetailsState["handleRemoveRedaction"];
  handleRemoveAllRedactions: CaseDetailsState["handleRemoveAllRedactions"];
  handleSavedRedactions: CaseDetailsState["handleSavedRedactions"];
  handleOpenPdfInNewTab: CaseDetailsState["handleOpenPdfInNewTab"];
};

export const PdfTabs: React.FC<PdfTabsProps> = ({
  tabsState: { items, headers },
  pipelineState,
  handleClosePdf,
  handleLaunchSearchResults,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  handleOpenPdfInNewTab,
}) => {
  return (
    <Tabs
      idPrefix="pdf"
      items={items.map((item) => ({
        isDirty: item.redactionHighlights.length > 0,
        id: item.tabSafeId,
        label: item.presentationFileName,
        panel: {
          children: (
            <PdfTab
              caseDocumentViewModel={item}
              redactStatus={getRedactStatus(item.documentId, pipelineState)}
              headers={headers}
              handleLaunchSearchResults={handleLaunchSearchResults}
              handleAddRedaction={handleAddRedaction}
              handleRemoveRedaction={handleRemoveRedaction}
              handleRemoveAllRedactions={handleRemoveAllRedactions}
              handleSavedRedactions={handleSavedRedactions}
              handleOpenPdfInNewTab={handleOpenPdfInNewTab}
            />
          ),
        },
      }))}
      title="Contents"
      handleClosePdf={handleClosePdf}
    />
  );
};
