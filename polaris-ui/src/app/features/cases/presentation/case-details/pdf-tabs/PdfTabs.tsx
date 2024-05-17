import { Tabs } from "../../../../../common/presentation/components/tabs";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfTab } from "./PdfTab";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { ISearchPIIHighlight } from "../../../domain/NewPdfHighlight";
import { SearchPIIData } from "../../../domain/gateway/SearchPIIData";

type PdfTabsProps = {
  redactionTypesData: RedactionTypeData[];
  tabsState: {
    items: CaseDocumentViewModel[];
    headers: HeadersInit;
    activeTabId: string | undefined;
  };

  savedDocumentDetails: {
    documentId: string;
    polarisDocumentVersionId: number;
  }[];
  contextData: {
    correlationId: string;
    searchPIIOn: string[];
    showSearchPII: boolean;
  };
  caseId: number;
  isOkToSave: boolean;
  showOverRedactionLog: boolean;
  searchPIIData: SearchPIIData[];
  handleOpenPdf: (caseDocument: {
    documentId: string;
    mode: "read" | "search";
  }) => void;
  handleTabSelection: (documentId: string) => void;
  handleClosePdf: (caseDocument: { documentId: string }) => void;
  handleLaunchSearchResults: () => void;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: CaseDetailsState["handleRemoveRedaction"];
  handleRemoveAllRedactions: CaseDetailsState["handleRemoveAllRedactions"];
  handleSavedRedactions: CaseDetailsState["handleSavedRedactions"];
  handleUnLockDocuments: CaseDetailsState["handleUnLockDocuments"];
  handleShowHideDocumentIssueModal: CaseDetailsState["handleShowHideDocumentIssueModal"];
  handleShowRedactionLogModal: CaseDetailsState["handleShowRedactionLogModal"];
  handleAreaOnlyRedaction: CaseDetailsState["handleAreaOnlyRedaction"];
  handleShowHideRedactionSuggestions: CaseDetailsState["handleShowHideRedactionSuggestions"];
  handleIgnoreRedactionSuggestion: CaseDetailsState["handleIgnoreRedactionSuggestion"];
};

export const PdfTabs: React.FC<PdfTabsProps> = ({
  redactionTypesData,
  caseId,
  tabsState: { items, headers, activeTabId },
  contextData,
  savedDocumentDetails,
  showOverRedactionLog,
  searchPIIData,
  handleTabSelection,
  isOkToSave,
  handleOpenPdf,
  handleClosePdf,
  handleLaunchSearchResults,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  handleUnLockDocuments,
  handleShowHideDocumentIssueModal,
  handleShowRedactionLogModal,
  handleAreaOnlyRedaction,
  handleShowHideRedactionSuggestions,
  handleIgnoreRedactionSuggestion,
}) => {
  return (
    <Tabs
      idPrefix="pdf"
      items={items.map((item, index) => ({
        isDirty: item.redactionHighlights.length > 0,
        id: item.documentId,
        label: item.presentationFileName,
        panel: {
          children: (
            <PdfTab
              caseId={caseId}
              searchPIIHighlights={
                searchPIIData
                  .find((data) => data.documentId === item.documentId)
                  ?.searchPIIHighlights.filter(
                    (highlight) => highlight.redactionStatus === "redacted"
                  ) ?? []
              }
              tabIndex={index}
              showOverRedactionLog={showOverRedactionLog}
              redactionTypesData={redactionTypesData}
              caseDocumentViewModel={item}
              savedDocumentDetails={savedDocumentDetails}
              documentWriteStatus={item.presentationFlags.write}
              headers={headers}
              isOkToSave={isOkToSave}
              handleOpenPdf={handleOpenPdf}
              handleLaunchSearchResults={handleLaunchSearchResults}
              handleAddRedaction={handleAddRedaction}
              handleRemoveRedaction={handleRemoveRedaction}
              handleRemoveAllRedactions={handleRemoveAllRedactions}
              handleSavedRedactions={handleSavedRedactions}
              handleShowHideDocumentIssueModal={
                handleShowHideDocumentIssueModal
              }
              handleShowRedactionLogModal={handleShowRedactionLogModal}
              handleAreaOnlyRedaction={handleAreaOnlyRedaction}
              handleShowHideRedactionSuggestions={
                handleShowHideRedactionSuggestions
              }
              handleIgnoreRedactionSuggestion={handleIgnoreRedactionSuggestion}
              contextData={contextData}
              activeTabId={activeTabId}
              tabId={item.documentId}
            />
          ),
        },
      }))}
      title="Contents"
      activeTabId={activeTabId}
      handleClosePdf={handleClosePdf}
      handleTabSelection={handleTabSelection}
      handleUnLockDocuments={handleUnLockDocuments}
    />
  );
};
