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
import { Wait as AccordionWait } from "./accordion/Wait";
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
import {
  PipelineDocument,
  Classification,
} from "../../domain/gateway/PipelineDocument";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  BULK_UM_REDIRECT_URL,
  CASE_REVIEW_APP_REDIRECT_URL,
  FEATURE_FLAG_REDACTION_LOG_UNDER_OVER,
} from "../../../../config";
import { AccordionReducerState } from "./accordion/reducer";
import { useSwitchContentArea } from "../../../../common/hooks/useSwitchContentArea";
import { useDocumentFocus } from "../../../../common/hooks/useDocumentFocus";
import { ReportAnIssueModal } from "./modals/ReportAnIssueModal";
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
export const path = "/case-details/:urn/:id";

type Props = BackLinkingPageProps & {
  context: TaggedContext | undefined;
};

export const Page: React.FC<Props> = ({ backLinkProps, context }) => {
  const [reclassifyDetails, setInReclassifyDetails] = useState<{
    open: boolean;
    documentId: string;
    presentationFileName: string;
    docTypeId: number | null;
    isUnused: boolean;
  }>({
    open: false,
    documentId: "",
    presentationFileName: "",
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
    presentationFileName: string;
    classification: Classification;
  }>({
    open: false,
    type: "",
    documentId: "",
    documentCategory: "",
    documentType: "",
    presentationFileName: "",
    classification: null,
  });

  const accordionRef = useRef<AccordionRef>(null);

  const [accordionOldState, setAccordionOldState] =
    useState<AccordionReducerState | null>(null);

  const actionsSidePanelRef = useRef(null);
  useAppInsightsTrackPageView("Case Details Page");
  const trackEvent = useAppInsightsTrackEvent();
  const history = useHistory();
  const { id: caseId, urn } = useParams<{ id: string; urn: string }>();

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
    caseState,
    accordionState,
    tabsState,
    searchState,
    searchTerm,
    pipelineState,
    pipelineRefreshData,
    errorModal,
    documentIssueModal,
    redactionLog,
    featureFlags,
    storedUserData,
    notes,
    searchPII,
    renameDocuments,
    reclassifyDocuments,
    notificationState,
    handleOpenPdf,
    handleClosePdf,
    handleTabSelection,
    handleSearchTermChange,
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
    handleShowHideDocumentIssueModal,
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
    handleClearAllNotifications,
    handleClearNotification,
  } = useCaseDetailsState(urn, +caseId, context, unMountingCallback);

  const {
    showAlert,
    setShowAlert,
    newPath,
    navigationUnblockHandle,
    unSavedRedactionDocs,
  } = useNavigationAlert(tabsState.items);

  useSwitchContentArea();
  useDocumentFocus(tabsState.activeTabId);

  useEffect(() => {
    if (accordionState.status === "succeeded") {
      const categorisedData = accordionState.data.reduce(
        (acc: { [key: string]: number }, curr) => {
          acc[`${curr.sectionId}`] = curr.docs.length;
          return acc;
        },
        {}
      );

      trackEvent("Categorised Documents Count", {
        ...categorisedData,
      });

      const unCategorisedDocs = accordionState.data.find(
        (accordionState) => accordionState.sectionId === "Uncategorised"
      );
      if (unCategorisedDocs) {
        unCategorisedDocs.docs.forEach((doc: MappedCaseDocument) => {
          trackEvent("Uncategorised Document", {
            documentId: doc.cmsDocumentId,
            documentTypeId: doc.cmsDocType.documentTypeId,
            documentDocumentType: doc.cmsDocType.documentType,
            fileName: doc.presentationFileName,
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

  const getActiveTabDocument = useMemo(() => {
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

  const accordionStateChangeCallback = useCallback(
    (state: AccordionReducerState) => {
      setAccordionOldState(state);
    },
    []
  );
  const handleClosePanel = useCallback(() => {
    setActionsSidePanel({
      ...actionsSidePanel,
      open: false,
      type: "",
      documentId: "",
      documentCategory: "",
      documentType: "",
      presentationFileName: "",
    });
  }, [actionsSidePanel]);

  if (caseState.status === "loading") {
    // if we are waiting on the main case details call, show holding message
    //  (we are prepared to show page whilst waiting for docs to load though)
    return <WaitPage />;
  }

  const isMultipleDefendantsOrCharges = isMultipleChargeCase(caseState.data);

  const dacDocumentId = getDACDocumentId(
    pipelineState?.haveData ? pipelineState.data.documents : []
  );

  const handleOpenPanel = (
    documentId: string,
    documentCategory: string,
    presentationFileName: string,
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
      presentationFileName: presentationFileName,
      classification: classification,
    });
  };

  const handleReclassifyDocument = (documentId: string) => {
    const selectedDocument =
      pipelineState.haveData &&
      (pipelineState.data.documents.find(
        (doc) => doc.documentId === documentId
      ) as PipelineDocument);
    if (selectedDocument) {
      handleResetReclassifyData(documentId);

      setInReclassifyDetails({
        open: true,
        documentId,
        presentationFileName: selectedDocument.presentationTitle,
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
    setInReclassifyDetails({
      open: false,
      documentId: "",
      presentationFileName: "",
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
        properties.exhibit.item !== reclassifyDetails.presentationFileName
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
                    : getActiveTabDocument?.documentId,
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

        {documentIssueModal.show && (
          <ReportAnIssueModal
            documentTypeId={getActiveTabDocument?.cmsDocType?.documentTypeId}
            documentId={getActiveTabDocument?.documentId!}
            presentationTitle={getActiveTabDocument?.presentationTitle!}
            polarisDocumentVersionId={
              getActiveTabDocument?.polarisDocumentVersionId!
            }
            correlationId={pipelineState?.correlationId}
            handleShowHideDocumentIssueModal={handleShowHideDocumentIssueModal}
          />
        )}
        {searchState.isResultsVisible && (
          <ResultsModal
            {...{
              caseState,
              searchTerm,
              searchState,
              pipelineState,
              handleSearchTermChange,
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
              documentName={getActiveTabDocument.presentationFileName}
              cmsDocumentTypeId={getActiveTabDocument.cmsDocType.documentTypeId}
              additionalData={{
                originalFileName: getActiveTabDocument.cmsOriginalFileName,
                documentId: getActiveTabDocument.documentId,
                documentType: getActiveTabDocument.cmsDocType.documentType,
                fileCreatedDate: getActiveTabDocument.cmsFileCreatedDate,
              }}
              savedRedactionTypes={redactionLog.savedRedactionTypes}
              saveStatus={getActiveTabDocument.saveStatus}
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
                  {featureFlags.externalRedirect && (
                    <div className={classes.externalRedirectBtnWrapper}>
                      <Button
                        disabled={false}
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
                        Case Review App <NewWindow />
                      </Button>

                      <Button
                        disabled={false}
                        onClick={() => {
                          openInNewTab(
                            `${BULK_UM_REDIRECT_URL}?URN=${urn}&CMSCaseId=${caseId}`
                          );
                        }}
                        data-testid="btn-bulk-um-classification"
                        id="btn-bulk-um-classification"
                        className={`${classes.newWindowBtn} govuk-button--secondary`}
                        name="secondary"
                      >
                        Bulk UM Classification <NewWindow />
                      </Button>
                    </div>
                  )}

                  {context && isTaggedTriageContext(context) && (
                    <div className={classes.externalRedirectBtnWrapper}>
                      <Button
                        disabled={false}
                        onClick={() => {
                          openInNewTab(
                            `/api/navigate-cms?action=activate_task&screen=case_details&taskId=${context.taskId}&caseId=${caseId}&wId=MASTER`
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
                    <AccordionWait />
                  ) : (
                    <Accordion
                      ref={accordionRef}
                      initialState={accordionOldState}
                      readUnreadData={
                        storedUserData.status === "succeeded"
                          ? storedUserData.data.readUnread
                          : []
                      }
                      accordionState={accordionState.data}
                      handleOpenPdf={(caseDoc) => {
                        handleOpenPdf({ ...caseDoc, mode: "read" });
                      }}
                      activeDocumentId={getActiveTabDocument?.documentId ?? ""}
                      handleOpenPanel={handleOpenPanel}
                      featureFlags={featureFlags}
                      accordionStateChangeCallback={
                        accordionStateChangeCallback
                      }
                      handleGetNotes={handleGetNotes}
                      notesData={notes}
                      handleReclassifyDocument={handleReclassifyDocument}
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
                        {`Notes panel, you can add and read notes for the document ${actionsSidePanel.presentationFileName}.`}
                      </span>
                      <NotesPanel
                        activeDocumentId={getActiveTabDocument?.documentId}
                        documentName={actionsSidePanel.presentationFileName}
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
                        {`Rename document panel, you can rename document ${actionsSidePanel.presentationFileName}.`}
                      </span>
                      <RenamePanel
                        documentName={actionsSidePanel.presentationFileName}
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
                          documentId: getActiveTabDocument.documentId,
                        });
                        setInFullScreen(false);
                      } else {
                        trackEvent("View Full Screen", {
                          documentId: getActiveTabDocument.documentId,
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
                  context={context}
                />
              ) : (
                <PdfTabs
                  searchPIIData={searchPII}
                  redactionTypesData={
                    redactionLog.redactionLogLookUpsData.status === "succeeded"
                      ? redactionLog.redactionLogLookUpsData.data
                          .missedRedactions
                      : []
                  }
                  isOkToSave={pipelineState.status === "complete"}
                  tabsState={tabsState}
                  savedDocumentDetails={
                    pipelineRefreshData.savedDocumentDetails
                  }
                  handleTabSelection={handleTabSelection}
                  handleClosePdf={handleClosePdf}
                  handleLaunchSearchResults={handleLaunchSearchResults}
                  handleAddRedaction={handleAddRedaction}
                  handleRemoveRedaction={handleRemoveRedaction}
                  handleRemoveAllRedactions={handleRemoveAllRedactions}
                  handleSavedRedactions={handleSavedRedactions}
                  handleOpenPdf={handleOpenPdf}
                  handleUnLockDocuments={handleUnLockDocuments}
                  handleShowHideDocumentIssueModal={
                    handleShowHideDocumentIssueModal
                  }
                  handleShowRedactionLogModal={handleShowRedactionLogModal}
                  handleShowHideRedactionSuggestions={
                    handleShowHideRedactionSuggestions
                  }
                  handleSearchPIIAction={handleSearchPIIAction}
                  contextData={{
                    correlationId: pipelineState?.correlationId,
                    showSearchPII: featureFlags.searchPII,
                    showDeletePage: featureFlags.pageDelete,
                  }}
                  caseId={+caseId}
                  showOverRedactionLog={
                    redactionLog.redactionLogLookUpsData.status === "succeeded"
                      ? FEATURE_FLAG_REDACTION_LOG_UNDER_OVER
                      : false
                  }
                  handleAreaOnlyRedaction={handleAreaOnlyRedaction}
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
            presentationTitle={reclassifyDetails.presentationFileName}
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
