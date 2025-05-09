import { useParams, useHistory } from "react-router-dom";
import { useEffect, useMemo, useState, useRef, useCallback } from "react";
import {
  BackLink,
  Tooltip,
  LinkButton,
  PageContentWrapper,
  WaitPage,
  Button,
} from "../../../../common/presentation/components";
import { Wait } from "./accordion/Wait";
import { BackLinkingPageProps } from "../../../../common/presentation/types/BackLinkingPageProps";
import { Accordion, AccordionRef } from "./accordion/Accordion";
import { KeyDetails } from "./KeyDetails";
import classes from "./index.module.scss";
import { PdfTabs } from "./pdf-tabs/PdfTabs";
import { useCaseDetailsState } from "../../hooks/use-case-details-state/useCaseDetailsState";
import { PdfTabsEmpty } from "./pdf-tabs/PdfTabsEmpty";
import { SearchBox } from "./search-box/SearchBox";
import { ResultsModal } from "./results/ResultsModal";
import { Charges } from "./Charges";
import { Modal } from "../../../../common/presentation/components/Modal";
import { NavigationAwayAlertContent } from "./navigation-alerts/NavigationAwayAlertContent";
import { useNavigationAlert } from "../../hooks/useNavigationAlert";
import {
  isMultipleChargeCase,
  getDACDocumentId,
} from "./utils/multipleChargeCaseUtils";
import { ErrorModalContent } from "../../../../common/presentation/components/ErrorModalContent";
import {
  useAppInsightsTrackEvent,
  useAppInsightsTrackPageView,
} from "../../../../common/hooks/useAppInsightsTracks";
import { Classification } from "../../domain/gateway/PipelineDocument";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  BULK_UM_REDIRECT_URL,
  CASE_REVIEW_APP_REDIRECT_URL,
  FEATURE_FLAG_REDACTION_LOG_UNDER_OVER,
} from "../../../../config";
import { useSwitchContentArea } from "../../../../common/hooks/useSwitchContentArea";
import { useDocumentFocus } from "../../../../common/hooks/useDocumentFocus";
import { RedactionLogModal } from "./redactionLog/RedactionLogModal";
import { NotesPanel } from "./notes/NotesPanel";
import { RenamePanel } from "./rename/RenamePanel";
import { Reclassify } from "./reclassify/Reclassify";
import { ReactComponent as DownArrow } from "../../../../common/presentation/svgs/down.svg";
import {
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  getWitnessStatementNumbers,
  saveDocumentReclassify,
} from "../../api/gateway-api";
import { ReclassifySaveData } from "../case-details/reclassify/data/ReclassifySaveData";
import { ReactComponent as NewWindow } from "../../../../common/presentation/svgs/new-window.svg";
import { Notifications } from "./notifications/Notifications";
import {
  isTaggedTriageContext,
  TaggedContext,
} from "../../../../inbound-handover/context";
import { saveStateToSessionStorage } from "./utils/stateRetentionUtil";

export const path = "/case-details/:urn/:id/:hkDocumentId?";

type Props = BackLinkingPageProps & {
  context: TaggedContext | undefined;
};

export const Page: React.FC<Props> = ({ backLinkProps, context }) => {
  const [reclassifyDetails, setReclassifyDetails] = useState<{
    open: boolean;
    documentId: string;
    presentationTitle: string;
    docTypeId: number | null;
    isUnused: boolean;
  }>({
    open: false,
    documentId: "",
    presentationTitle: "",
    docTypeId: null,
    isUnused: false,
  });
  const [inFullScreen, setInFullScreen] = useState(false);
  const [actionsSidePanel, setActionsSidePanel] = useState<{
    open: boolean;
    type: "notes" | "rename" | "";
    documentId: string;
    documentCategory: string;
    documentType: string;
    presentationTitle: string;
    classification: Classification;
  }>({
    open: false,
    type: "",
    documentId: "",
    documentCategory: "",
    documentType: "",
    presentationTitle: "",
    classification: null,
  });

  const accordionRef = useRef<AccordionRef>(null);

  const actionsSidePanelRef = useRef(null);
  useAppInsightsTrackPageView("Case Details Page");
  const trackEvent = useAppInsightsTrackEvent();
  const history = useHistory();
  const params = useParams<{ id: string; urn: string; hkDocumentId: string }>();
  const { id: caseId, urn, hkDocumentId } = params as any;

  const unMounting = useRef(false);
  useEffect(() => {
    return () => {
      unMounting.current = true;
    };
  }, []);
  const unMountingCallback = useCallback(() => {
    return unMounting.current;
  }, []);

  const {
    combinedState,
    handleOpenPdf,
    handleClosePdf,
    handleTabSelection,
    handleSearchTermChange,
    handleSearchTypeChange,
    handleLaunchSearchResults,
    handleCloseSearchResults,
    handleChangeResultsOrder,
    handleUpdateFilter,
    handleAddRedaction,
    handleRemoveRedaction,
    handleRemoveAllRedactions,
    handleSavedRedactions,
    handleSaveRedactionLog,
    handleCloseErrorModal,
    handleUnLockDocuments,
    handleShowRedactionLogModal,
    handleHideRedactionLogModal,
    handleAreaOnlyRedaction,
    handleGetNotes,
    handleAddNote,
    handleSaveRename,
    handleShowHideRedactionSuggestions,
    handleSearchPIIAction,
    handleResetRenameData,
    handleReclassifySuccess,
    handleResetReclassifyData,
    handleShowHidePageRotation,
    handleAddPageRotation,
    handleRemovePageRotation,
    handleRemoveAllRotations,
    handleSaveRotations,
    handleClearAllNotifications,
    handleClearNotification,
    handleUpdateConversionStatus,
    handleShowHidePageDeletion,
    handleHideSaveRotationModal,
    handleAccordionOpenClose,
    handleAccordionOpenCloseAll,
  } = useCaseDetailsState(urn, +caseId, context, unMountingCallback);

  const {
    context: stateContext,
    caseState,
    accordionState,
    tabsState,
    searchState,
    searchTerm,
    pipelineState,
    documentsState,
    documentRefreshData,
    errorModal,
    redactionLog,
    featureFlags,
    storedUserData,
    notes,
    searchPII,
    renameDocuments,
    reclassifyDocuments,
    notificationState,
    localDocumentState,
  } = combinedState;
  useEffect(() => {
    if (featureFlags.stateRetention) {
      saveStateToSessionStorage(combinedState);
    }
  }, [combinedState, featureFlags.stateRetention]);

  const {
    showAlert,
    setShowAlert,
    newPath,
    navigationUnblockHandle,
    unSavedRedactionDocs,
  } = useNavigationAlert(tabsState.items, documentsState);

  useSwitchContentArea();
  useDocumentFocus(tabsState.activeTabId);

  useEffect(() => {
    if (accordionState.status === "succeeded") {
      const categorisedData = accordionState.data.sections.reduce(
        (acc: { [key: string]: number }, curr) => {
          acc[`${curr.sectionId}`] = curr.docs.length;
          return acc;
        },
        {}
      );

      trackEvent("Categorised Documents Count", {
        ...categorisedData,
      });

      const unCategorisedDocs = accordionState.data.sections.find(
        (accordionState) => accordionState.sectionId === "Uncategorised"
      );

      if (unCategorisedDocs && documentsState.status === "succeeded") {
        const mappedUnCategorisedDocs = unCategorisedDocs.docs.map(
          ({ documentId }) =>
            documentsState.data.find((doc) => doc.documentId === documentId)!
        );
        mappedUnCategorisedDocs.forEach((doc: MappedCaseDocument) => {
          trackEvent("Uncategorised Document", {
            documentId: doc.documentId,
            documentTypeId: doc.cmsDocType.documentTypeId,
            documentDocumentType: doc.cmsDocType.documentType,
            fileName: doc.presentationTitle,
          });
        });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accordionState.status]);

  useEffect(() => {
    trackEvent("Open Documents Count", {
      count: tabsState.items.length,
    });
    if (tabsState.items.length === 0) {
      setInFullScreen(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tabsState.items.length]);

  useEffect(() => {
    if (actionsSidePanelRef.current) {
      (actionsSidePanelRef.current as HTMLElement).focus();
    }
  }, [actionsSidePanel.open]);

  const activeTabMappedDocument = useMemo(() => {
    const mappedDocuments =
      documentsState.status === "succeeded" ? documentsState.data : [];
    return mappedDocuments.find(
      (item) => item.documentId === tabsState.activeTabId
    )!;
  }, [tabsState.activeTabId, documentsState]);

  const activeTabItem = useMemo(() => {
    return tabsState.items.find(
      (item) => item.documentId === tabsState.activeTabId
    )!;
  }, [tabsState.activeTabId, tabsState.items]);

  const activeReclassifyDocumentUpdated = useCallback(
    (documentId: string) => {
      const activeReclassifyDoc = reclassifyDocuments.find(
        (item) => item.documentId === documentId
      );
      return activeReclassifyDoc?.saveReclassifyRefreshStatus === "updated";
    },
    [reclassifyDocuments]
  );

  const handleClosePanel = useCallback(() => {
    setActionsSidePanel({
      ...actionsSidePanel,
      open: false,
      type: "",
      documentId: "",
      documentCategory: "",
      documentType: "",
      presentationTitle: "",
    });
  }, [actionsSidePanel]);

  if (caseState.status === "loading") {
    // if we are waiting on the main case details call, show holding message
    //  (we are prepared to show page whilst waiting for docs to load though)
    return (
      <>
        <WaitPage />
      </>
    );
  }

  const isMultipleDefendantsOrCharges = isMultipleChargeCase(caseState.data);

  const dacDocumentId = getDACDocumentId(
    documentsState?.status === "succeeded" ? documentsState.data : []
  );

  const handleOpenPanel = (
    documentId: string,
    documentCategory: string,
    presentationTitle: string,
    type: "notes" | "rename",
    documentType: string,
    classification: Classification
  ) => {
    setActionsSidePanel({
      open: true,
      type: type,
      documentId: documentId,
      documentCategory: documentCategory,
      documentType: documentType,
      presentationTitle,
      classification: classification,
    });
  };

  const handleReclassifyDocument = (documentId: string) => {
    const selectedDocument =
      documentsState?.status === "succeeded" &&
      (documentsState.data.find(
        (doc) => doc.documentId === documentId
      ) as MappedCaseDocument);
    if (selectedDocument) {
      handleResetReclassifyData(documentId);

      setReclassifyDetails({
        open: true,
        documentId,
        presentationTitle: selectedDocument.presentationTitle,
        docTypeId: selectedDocument.cmsDocType.documentTypeId,
        isUnused: selectedDocument.isUnused,
      });
    }
  };

  const handleOpenAccordion = (documentId: string) => {
    if (accordionRef.current) {
      accordionRef.current.handleOpenAccordion(documentId);
    }
  };

  const handleCloseReclassify = (documentId: string) => {
    setReclassifyDetails({
      open: false,
      documentId: "",
      presentationTitle: "",
      docTypeId: null,
      isUnused: false,
    });
    handleOpenAccordion(documentId);
    setTimeout(() => {
      (
        document.querySelector(
          `#document-housekeeping-actions-dropdown-${reclassifyDetails.documentId}`
        ) as HTMLElement
      ).focus();
    }, 500);
  };

  const handleGetMaterialTypeList = () => {
    return getMaterialTypeList();
  };
  const handleGetExhibitProducers = () => {
    return getExhibitProducers(urn, +caseId);
  };
  const handleGetStatementWitnessDetails = () => {
    return getStatementWitnessDetails(urn, +caseId);
  };

  const handleGetWitnessStatementNumbers = (witnessId: number) => {
    return getWitnessStatementNumbers(urn, +caseId, witnessId);
  };

  const handleSubmitReclassify = async (
    documentId: string,
    data: ReclassifySaveData
  ) => {
    const response = await saveDocumentReclassify(
      urn,
      +caseId,
      documentId,
      data
    );
    if (response) {
      const wasDocumentRenamed = !!(
        data.exhibit ||
        data.statement ||
        data?.immediate?.documentName ||
        data?.other?.documentName
      );

      handleReclassifySuccess(
        documentId,
        data.documentTypeId,
        wasDocumentRenamed
      );
    }
    return response;
  };

  const handleReclassifyTracking = (
    name: string,
    properties: Record<string, any>
  ) => {
    if (name !== "Save Reclassify" && name !== "Save Reclassify Error") {
      return;
    }

    const getNameChanged = () => {
      if (properties?.immediate?.documentName) {
        return true;
      }
      if (properties?.other?.documentName) {
        return true;
      }
      if (
        properties.exhibit &&
        properties.exhibit.item !== reclassifyDetails.presentationTitle
      ) {
        return true;
      }
      return false;
    };
    const getNewIsUnused = () => {
      if (properties.other) {
        return !properties.other.used;
      }
      if (properties.exhibit) {
        return !properties.exhibit.used;
      }
      if (properties.statement) {
        return !properties.statement.used;
      }
      return null;
    };
    const trackingProperties = {
      documentId: properties.documentId,
      oldDocumentType: reclassifyDetails.docTypeId,
      newDocumentType: properties.documentTypeId,
      nameChanged: getNameChanged(),
      oldIsUnused: reclassifyDetails.isUnused,
      newIsUnused: getNewIsUnused(),
    };

    trackEvent(name, trackingProperties);
  };

  const openInNewTab = (url: string) => {
    window.open(url, "_blank", "noopener,noreferrer");
  };
  const openInSameTab = (url: string) => {
    window.open(url, "_self");
  };

  return (
    <div>
      <div className={reclassifyDetails.open ? classes.reclassifyMode : ""}>
        {errorModal.show && (
          <Modal
            isVisible
            handleClose={handleCloseErrorModal}
            type="alert"
            ariaLabel="Error Modal"
            ariaDescription={`${
              errorModal.title
            } ${errorModal.message.replaceAll("</p>", "")}`}
          >
            <ErrorModalContent
              title={errorModal.title}
              message={errorModal.message}
              type={errorModal.type}
              handleClose={handleCloseErrorModal}
              contextData={{
                documentId:
                  errorModal.type === "addnote" ||
                  errorModal.type === "saverenamedocument"
                    ? actionsSidePanel.documentId
                    : activeTabMappedDocument?.documentId,
              }}
            />
          </Modal>
        )}
        {showAlert && (
          <Modal
            isVisible
            handleClose={() => {
              setShowAlert(false);
            }}
            type="alert"
            ariaLabel="Unsaved redaction warning modal"
            ariaDescription="You are navigating away from documents with unsaved redactions"
          >
            <NavigationAwayAlertContent
              type="casefile"
              unSavedRedactionDocs={unSavedRedactionDocs}
              handleCancelAction={() => {
                setShowAlert(false);
              }}
              handleContinueAction={(documentIds) => {
                setShowAlert(false);
                handleUnLockDocuments(documentIds);
                navigationUnblockHandle.current();
                history.push(newPath);
              }}
              handleOpenPdf={(params) => {
                setShowAlert(false);
                handleOpenPdf({ ...params, mode: "read" });
              }}
            />
          </Modal>
        )}

        {searchState.isResultsVisible && (
          <ResultsModal
            {...{
              caseState,
              searchTerm,
              searchState,
              pipelineState,
              featureFlags,
              handleSearchTermChange,
              handleSearchTypeChange,
              handleLaunchSearchResults,
              handleCloseSearchResults,
              handleChangeResultsOrder,
              handleUpdateFilter,
              handleOpenPdf: (caseDoc) => {
                handleOpenPdf(caseDoc);
              },
            }}
          />
        )}

        {redactionLog.showModal &&
          redactionLog.redactionLogLookUpsData.status === "succeeded" && (
            <RedactionLogModal
              redactionLogType={redactionLog.type}
              caseUrn={caseState.data.uniqueReferenceNumber}
              isCaseCharged={caseState.data.isCaseCharged}
              owningUnit={caseState.data.owningUnit}
              documentName={activeTabMappedDocument.presentationTitle}
              cmsDocumentTypeId={
                activeTabMappedDocument.cmsDocType.documentTypeId
              }
              additionalData={{
                originalFileName: activeTabMappedDocument.cmsOriginalFileName,
                documentId: activeTabMappedDocument.documentId,
                documentType: activeTabMappedDocument.cmsDocType.documentType,
                fileCreatedDate: activeTabMappedDocument.cmsFileCreatedDate,
              }}
              savedRedactionTypes={redactionLog.savedRedactionTypes}
              saveStatus={activeTabItem.saveStatus}
              redactionLogLookUpsData={
                redactionLog.redactionLogLookUpsData.data
              }
              handleSaveRedactionLog={handleSaveRedactionLog}
              redactionLogMappingsData={
                redactionLog.redactionLogMappingData.status === "succeeded"
                  ? redactionLog.redactionLogMappingData.data
                  : null
              }
              handleHideRedactionLogModal={handleHideRedactionLogModal}
            />
          )}
        <nav>
          <BackLink
            to={backLinkProps.to}
            onClick={() => trackEvent("Back to Case Search Results")}
          >
            {backLinkProps.label}
          </BackLink>
          {featureFlags.notifications && (
            <Notifications
              state={notificationState}
              handleOpenPdf={handleOpenPdf}
              handleClearAllNotifications={handleClearAllNotifications}
              handleClearNotification={handleClearNotification}
            ></Notifications>
          )}
        </nav>
        <PageContentWrapper>
          <div
            className={`govuk-grid-row ${classes.mainContent} ${
              featureFlags.notifications
                ? classes.mainContentWithNotifications
                : ""
            }`}
          >
            {!inFullScreen && !actionsSidePanel.open && (
              <div
                role="region"
                aria-labelledby="side-panel-region-label"
                id="side-panel"
                data-testid="side-panel"
                // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
                tabIndex={0}
                className={`govuk-grid-column-one-quarter perma-scrollbar ${classes.leftColumn} ${classes.sidePanelArea}`}
              >
                <span
                  id="side-panel-region-label"
                  className={classes.sidePanelLabel}
                >
                  Case navigation panel
                </span>
                <div>
                  <KeyDetails
                    handleOpenPdf={() => {
                      handleOpenPdf({
                        documentId: dacDocumentId,
                        mode: "read",
                      });
                    }}
                    caseDetails={caseState.data}
                    isMultipleDefendantsOrCharges={
                      isMultipleDefendantsOrCharges
                    }
                    dacDocumentId={dacDocumentId}
                  />

                  {!isMultipleDefendantsOrCharges && (
                    <Charges caseDetails={caseState.data} />
                  )}
                  {
                    <div className={classes.externalRedirectBtnWrapper}>
                      {featureFlags.externalRedirectCaseReviewApp && (
                        <Button
                          onClick={() => {
                            openInNewTab(
                              `${CASE_REVIEW_APP_REDIRECT_URL}?URN=${urn}&CMSCaseId=${caseId}`
                            );
                          }}
                          data-testid="btn-case-review-app"
                          id="btn-case-review-app"
                          className={`${classes.newWindowBtn} govuk-button--secondary`}
                          name="secondary"
                        >
                          Case Review <NewWindow />
                        </Button>
                      )}

                      {featureFlags.externalRedirectBulkUmApp && (
                        <Button
                          onClick={() => {
                            openInSameTab(`${BULK_UM_REDIRECT_URL}/${caseId}`);
                          }}
                          data-testid="btn-bulk-um-classification"
                          id="btn-bulk-um-classification"
                          className={`${classes.newWindowBtn} govuk-button--secondary`}
                          name="secondary"
                        >
                          Bulk UM Classification
                        </Button>
                      )}
                    </div>
                  }

                  {stateContext && isTaggedTriageContext(stateContext) && (
                    <div className={classes.externalRedirectBtnWrapper}>
                      <Button
                        disabled={false}
                        onClick={() => {
                          openInNewTab(
                            `/api/navigate-cms?action=activate_task&screen=case_details&taskId=${stateContext.taskId}&caseId=${caseId}&wId=MASTER`
                          );
                        }}
                        data-testid="btn-bulk-um-classification"
                        id="btn-bulk-um-classification"
                        className={`${classes.newWindowBtn} govuk-button--secondary`}
                        name="secondary"
                      >
                        Complete triage on CMS <NewWindow />
                      </Button>
                    </div>
                  )}
                  {/* <div className={classes.externalRedirectBtnWrapper}>
                    <Button
                      disabled={false}
                      onClick={() => {
                        openInNewTab(
                          `${window.location.pathname}?URN=${urn}&caseId=${caseId}`
                        );
                      }}
                      data-testid="btn-housekeep-link"
                      id="btn-housekeep-link"
                      className={`${classes.newWindowBtn} govuk-button--secondary`}
                      name="secondary"
                    >
                      Housekeeping link <NewWindow />
                    </Button>
                  </div> */}
                  <SearchBox
                    id="case-details-search"
                    data-testid="search-case"
                    labelText="Search"
                    value={searchTerm}
                    handleChange={handleSearchTermChange}
                    handleSubmit={handleLaunchSearchResults}
                    trackEventKey="Search Case Documents From Case Details"
                  />

                  {accordionState.status === "loading" ? (
                    <Wait />
                  ) : (
                    <Accordion
                      ref={accordionRef}
                      documentsState={
                        documentsState.status === "succeeded"
                          ? documentsState.data
                          : []
                      }
                      readUnreadData={
                        storedUserData.status === "succeeded"
                          ? storedUserData.data.readUnread
                          : []
                      }
                      accordionState={accordionState.data}
                      handleOpenPdf={(caseDoc) => {
                        handleOpenPdf({ ...caseDoc, mode: "read" });
                      }}
                      activeDocumentId={
                        activeTabMappedDocument?.documentId ?? ""
                      }
                      handleOpenPanel={handleOpenPanel}
                      featureFlags={featureFlags}
                      handleGetNotes={handleGetNotes}
                      notesData={notes}
                      handleReclassifyDocument={handleReclassifyDocument}
                      localDocumentState={localDocumentState}
                      handleAccordionOpenClose={handleAccordionOpenClose}
                      handleAccordionOpenCloseAll={handleAccordionOpenCloseAll}
                      hkDocumentId={hkDocumentId}
                    />
                  )}
                </div>
              </div>
            )}
            {!inFullScreen && actionsSidePanel.open && (
              <>
                <div
                  className={`govuk-grid-column-one-quarter perma-scrollbar ${classes.leftColumn} ${classes.notesArea}`}
                  id="actions-panel"
                  role="region"
                  aria-labelledby="actions-panel-region-label"
                  // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
                  tabIndex={0}
                  ref={actionsSidePanelRef}
                  data-testid="actions-panel"
                >
                  {actionsSidePanel.type === "notes" && (
                    <div>
                      <span
                        id="actions-panel-region-label"
                        className={classes.sidePanelLabel}
                      >
                        {`Notes panel, you can add and read notes for the document ${actionsSidePanel.presentationTitle}.`}
                      </span>
                      <NotesPanel
                        activeDocumentId={activeTabMappedDocument?.documentId}
                        documentName={actionsSidePanel.presentationTitle}
                        documentCategory={actionsSidePanel.documentCategory}
                        documentId={actionsSidePanel.documentId}
                        notesData={notes}
                        handleCloseNotes={handleClosePanel}
                        handleAddNote={handleAddNote}
                        handleGetNotes={handleGetNotes}
                      />
                    </div>
                  )}
                  {actionsSidePanel.type === "rename" && (
                    <div>
                      <span
                        id="actions-panel-region-label"
                        className={classes.sidePanelLabel}
                      >
                        {`Rename document panel, you can rename document ${actionsSidePanel.presentationTitle}.`}
                      </span>
                      <RenamePanel
                        documentName={actionsSidePanel.presentationTitle}
                        documentType={actionsSidePanel.documentType}
                        documentId={actionsSidePanel.documentId}
                        classification={actionsSidePanel.classification}
                        renameDocuments={renameDocuments}
                        handleClose={handleClosePanel}
                        handleSaveRename={handleSaveRename}
                        handleResetRenameData={handleResetRenameData}
                      />
                    </div>
                  )}
                </div>
              </>
            )}
            {!!tabsState.items.length && featureFlags.fullScreen && (
              <div className={classes.resizeBtnWrapper}>
                <Tooltip
                  text={inFullScreen ? "Exit full screen" : "View full screen"}
                  position="right"
                >
                  <LinkButton
                    id={"full-screen-btn"}
                    dataTestId={"full-screen-btn"}
                    ariaLabel={
                      inFullScreen ? "Exit full screen" : "View full screen"
                    }
                    className={`${classes.resizeBtn} ${
                      inFullScreen && classes.inFullScreen
                    }`}
                    onClick={() => {
                      if (inFullScreen) {
                        trackEvent("Exit Full Screen", {
                          documentId: activeTabMappedDocument.documentId,
                        });
                        setInFullScreen(false);
                      } else {
                        trackEvent("View Full Screen", {
                          documentId: activeTabMappedDocument.documentId,
                        });
                        setInFullScreen(true);
                      }
                    }}
                  >
                    <DownArrow />
                  </LinkButton>
                </Tooltip>
              </div>
            )}
            <div
              className={`${classes.rightColumn} ${
                inFullScreen
                  ? "govuk-grid-column-full"
                  : "govuk-grid-column-three-quarters"
              }`}
            >
              {!tabsState.items.length ? (
                <PdfTabsEmpty
                  pipelineState={pipelineState}
                  isMultipleDefendantsOrCharges={isMultipleDefendantsOrCharges}
                  context={stateContext}
                />
              ) : (
                <PdfTabs
                  mappedCaseDocuments={
                    documentsState.status === "succeeded"
                      ? documentsState.data
                      : []
                  }
                  searchPIIData={searchPII}
                  redactionTypesData={
                    redactionLog.redactionLogLookUpsData.status === "succeeded"
                      ? redactionLog.redactionLogLookUpsData.data
                          .missedRedactions
                      : []
                  }
                  tabsState={tabsState}
                  savedDocumentDetails={
                    documentRefreshData.savedDocumentDetails
                  }
                  localDocumentState={localDocumentState}
                  handleTabSelection={handleTabSelection}
                  handleClosePdf={handleClosePdf}
                  handleLaunchSearchResults={handleLaunchSearchResults}
                  handleAddRedaction={handleAddRedaction}
                  handleRemoveRedaction={handleRemoveRedaction}
                  handleRemoveAllRedactions={handleRemoveAllRedactions}
                  handleSavedRedactions={handleSavedRedactions}
                  handleOpenPdf={handleOpenPdf}
                  handleUnLockDocuments={handleUnLockDocuments}
                  handleShowRedactionLogModal={handleShowRedactionLogModal}
                  handleShowHideRedactionSuggestions={
                    handleShowHideRedactionSuggestions
                  }
                  handleSearchPIIAction={handleSearchPIIAction}
                  featureFlags={featureFlags}
                  caseId={+caseId}
                  showOverRedactionLog={
                    redactionLog.redactionLogLookUpsData.status === "succeeded"
                      ? FEATURE_FLAG_REDACTION_LOG_UNDER_OVER
                      : false
                  }
                  handleAreaOnlyRedaction={handleAreaOnlyRedaction}
                  handleShowHidePageRotation={handleShowHidePageRotation}
                  handleAddPageRotation={handleAddPageRotation}
                  handleRemovePageRotation={handleRemovePageRotation}
                  handleRemoveAllRotations={handleRemoveAllRotations}
                  handleSaveRotations={handleSaveRotations}
                  handleUpdateConversionStatus={handleUpdateConversionStatus}
                  handleShowHidePageDeletion={handleShowHidePageDeletion}
                  handleHideSaveRotationModal={handleHideSaveRotationModal}
                />
              )}
            </div>
          </div>
        </PageContentWrapper>
      </div>
      {reclassifyDetails.open && (
        <div>
          <Reclassify
            documentId={reclassifyDetails.documentId}
            currentDocTypeId={reclassifyDetails.docTypeId}
            presentationTitle={reclassifyDetails.presentationTitle}
            reclassifiedDocumentUpdate={activeReclassifyDocumentUpdated(
              reclassifyDetails.documentId
            )}
            handleCloseReclassify={handleCloseReclassify}
            getMaterialTypeList={handleGetMaterialTypeList}
            getExhibitProducers={handleGetExhibitProducers}
            getStatementWitnessDetails={handleGetStatementWitnessDetails}
            getWitnessStatementNumbers={handleGetWitnessStatementNumbers}
            handleSubmitReclassify={handleSubmitReclassify}
            handleReclassifyTracking={handleReclassifyTracking}
          />
        </div>
      )}
    </div>
  );
};

export default Page;
