import { useParams, useHistory } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import {
  BackLink,
  Tooltip,
  LinkButton,
  PageContentWrapper,
  WaitPage,
  PhaseBanner,
} from "../../../../common/presentation/components";
import { Wait as AccordionWait } from "./accordion/Wait";
import { BackLinkingPageProps } from "../../../../common/presentation/types/BackLinkingPageProps";
import { Accordion } from "./accordion/Accordion";
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
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  SURVEY_LINK,
  FEATURE_FLAG_REDACTION_LOG_UNDER_OVER,
} from "../../../../config";
import { useSwitchContentArea } from "../../../../common/hooks/useSwitchContentArea";
import { useDocumentFocus } from "../../../../common/hooks/useDocumentFocus";
import { ReportAnIssueModal } from "./modals/ReportAnIssueModal";
import { RedactionLogModal } from "./redactionLog/RedactionLogModal";
import { ReactComponent as DownArrow } from "../../../../common/presentation/svgs/down.svg";
export const path = "/case-details/:urn/:id";

type Props = BackLinkingPageProps & {};

export const Page: React.FC<Props> = ({ backLinkProps }) => {
  const [inFullScreen, setInFullScreen] = useState(false);
  useAppInsightsTrackPageView("Case Details Page");
  const trackEvent = useAppInsightsTrackEvent();
  const history = useHistory();
  const { id: caseId, urn } = useParams<{ id: string; urn: string }>();

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
  } = useCaseDetailsState(urn, +caseId);

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

  const getActiveTabDocument = useMemo(() => {
    return tabsState.items.find(
      (item) => item.documentId === tabsState.activeTabId
    )!;
  }, [tabsState.activeTabId, tabsState.items]);

  if (caseState.status === "loading") {
    // if we are waiting on the main case details call, show holding message
    //  (we are prepared to show page whilst waiting for docs to load though)
    return <WaitPage />;
  }

  const isMultipleDefendantsOrCharges = isMultipleChargeCase(caseState.data);

  const dacDocumentId = getDACDocumentId(
    pipelineState?.haveData ? pipelineState.data.documents : []
  );

  return (
    <>
      {errorModal.show && (
        <Modal
          isVisible
          handleClose={handleCloseErrorModal}
          type="alert"
          ariaLabel="Error Modal"
          ariaDescription={`${errorModal.title} ${errorModal.message}`}
        >
          <ErrorModalContent
            title={errorModal.title}
            message={errorModal.message}
            handleClose={handleCloseErrorModal}
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
            redactionLogLookUpsData={redactionLog.redactionLogLookUpsData.data}
            handleSaveRedactionLog={handleSaveRedactionLog}
            redactionLogMappingsData={
              redactionLog.redactionLogMappingData.status === "succeeded"
                ? redactionLog.redactionLogMappingData.data
                : null
            }
            handleHideRedactionLogModal={handleHideRedactionLogModal}
            defaultLastFocus={
              document.querySelector("#active-tab-panel") as HTMLElement
            }
          />
        )}
      <nav>
        <PhaseBanner
          className={classes["phaseBanner"]}
          data-testid="feedback-banner"
        >
          Your{" "}
          <a
            className="govuk-link"
            href={SURVEY_LINK}
            target="_blank"
            rel="noreferrer"
          >
            feedback (opens in a new tab)
          </a>{" "}
          will help us to improve this service.
        </PhaseBanner>
        <BackLink
          to={backLinkProps.to}
          onClick={() => trackEvent("Back to Case Search Results")}
        >
          {backLinkProps.label}
        </BackLink>
      </nav>
      <PageContentWrapper>
        <div className={`govuk-grid-row ${classes.mainContent}`}>
          {!inFullScreen && (
            <div
              role="region"
              aria-labelledby="side-panel-region-label"
              id="side-panel"
              data-testid="side-panel"
              // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
              tabIndex={0}
              className={`govuk-grid-column-one-quarter perma-scrollbar ${classes.leftColumn} ${classes.contentArea}`}
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
                    handleOpenPdf({ documentId: dacDocumentId, mode: "read" });
                  }}
                  caseDetails={caseState.data}
                  isMultipleDefendantsOrCharges={isMultipleDefendantsOrCharges}
                  dacDocumentId={dacDocumentId}
                />

                {!isMultipleDefendantsOrCharges && (
                  <Charges caseDetails={caseState.data} />
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
                    readUnreadData={
                      storedUserData.status === "succeeded"
                        ? storedUserData.data.readUnread
                        : []
                    }
                    accordionState={accordionState.data}
                    handleOpenPdf={(caseDoc) => {
                      handleOpenPdf({ ...caseDoc, mode: "read" });
                    }}
                  />
                )}
              </div>
            </div>
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
              <PdfTabsEmpty pipelineState={pipelineState} />
            ) : (
              <PdfTabs
                redactionTypesData={
                  redactionLog.redactionLogLookUpsData.status === "succeeded"
                    ? redactionLog.redactionLogLookUpsData.data.missedRedactions
                    : []
                }
                isOkToSave={pipelineState.status === "complete"}
                tabsState={tabsState}
                savedDocumentDetails={pipelineRefreshData.savedDocumentDetails}
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
                contextData={{
                  correlationId: pipelineState?.correlationId,
                }}
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
    </>
  );
};

export default Page;
