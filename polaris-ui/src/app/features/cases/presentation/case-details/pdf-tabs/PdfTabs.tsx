import { useCallback } from "react";
import { Tabs } from "../../../../../common/presentation/components/tabs";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfTab } from "./PdfTab";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { SearchPIIData } from "../../../domain/gateway/SearchPIIData";
import { LocalDocumentState } from "../../../domain/LocalDocumentState";
import { FeatureFlagData } from "../../../domain/FeatureFlagData";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";

type PdfTabsProps = {
  mappedCaseDocuments: MappedCaseDocument[];
  redactionTypesData: RedactionTypeData[];
  tabsState: {
    items: CaseDocumentViewModel[];
    headers: HeadersInit;
    activeTabId: string | undefined;
  };

  savedDocumentDetails: {
    documentId: string;
    versionId: number;
  }[];
  featureFlags: FeatureFlagData;
  caseId: number;
  showOverRedactionLog: boolean;
  searchPIIData: SearchPIIData[];
  localDocumentState: LocalDocumentState;
  handleOpenPdf: (caseDocument: {
    documentId: string;
    mode: "read" | "search";
  }) => void;
  handleTabSelection: (documentId: string) => void;
  handleClosePdf: (documentId: string) => void;
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

export const PdfTabs: React.FC<PdfTabsProps> = ({
  mappedCaseDocuments,
  redactionTypesData,
  caseId,
  tabsState: { items, headers, activeTabId },
  featureFlags,
  savedDocumentDetails,
  showOverRedactionLog,
  searchPIIData,
  localDocumentState,
  handleTabSelection,
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
  const localHandleClosePdf = useCallback(
    (documentId: string, versionId: number) => {
      handleClosePdf(documentId);
      handleShowHideRedactionSuggestions(
        documentId,
        versionId,
        false,
        false,
        true
      );
    },
    [handleClosePdf, handleShowHideRedactionSuggestions]
  );

  const getMappedDocument = (
    mappedCaseDocuments: MappedCaseDocument[],
    documentId: string
  ) => {
    return mappedCaseDocuments.find((item) => item.documentId === documentId)!;
  };

  return (
    <Tabs
      idPrefix="pdf"
      items={items.map((item, index) => ({
        isDirty:
          item.redactionHighlights.length + item.pageDeleteRedactions.length >
          0,
        id: item.documentId,
        versionId: getMappedDocument(mappedCaseDocuments, item.documentId)
          ?.versionId,
        label:
          getMappedDocument(mappedCaseDocuments, item.documentId)
            ?.presentationTitle ?? "Deleted",
        panel: {
          children: (
            <PdfTab
              mappedDocument={
                getMappedDocument(mappedCaseDocuments, item.documentId) ?? {} //passing in the default value in case of a document opened is been deleted.
              }
              caseId={caseId}
              searchPIIDataItem={searchPIIData.find(
                (data) =>
                  data.documentId === item.documentId &&
                  data.versionId ===
                    getMappedDocument(mappedCaseDocuments, item.documentId)
                      .versionId
              )}
              tabIndex={index}
              showOverRedactionLog={showOverRedactionLog}
              redactionTypesData={redactionTypesData}
              caseDocumentViewModel={item}
              savedDocumentDetails={savedDocumentDetails}
              headers={headers}
              localDocumentState={localDocumentState}
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
              handleSearchPIIAction={handleSearchPIIAction}
              handleShowHidePageRotation={handleShowHidePageRotation}
              handleShowHidePageDeletion={handleShowHidePageDeletion}
              handleAddPageRotation={handleAddPageRotation}
              handleRemovePageRotation={handleRemovePageRotation}
              activeTabId={activeTabId}
              tabId={item.documentId}
              featureFlags={featureFlags}
              handleRemoveAllRotations={handleRemoveAllRotations}
              handleSaveRotations={handleSaveRotations}
              handleUpdateConversionStatus={handleUpdateConversionStatus}
              handleHideSaveRotationModal={handleHideSaveRotationModal}
            />
          ),
        },
      }))}
      title="Contents"
      activeTabId={activeTabId}
      handleClosePdf={localHandleClosePdf}
      handleTabSelection={handleTabSelection}
      handleUnLockDocuments={handleUnLockDocuments}
    />
  );
};
