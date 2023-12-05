import { useParams, useHistory } from "react-router-dom";
import { useEffect } from "react";
import { BackLink } from "../../../../common/presentation/components";
import { PageContentWrapper } from "../../../../common/presentation/components";
import {
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
import { SURVEY_LINK } from "../../../../config";
import { useSwitchContentArea } from "../../../../common/hooks/useSwitchContentArea";
import { useDocumentFocus } from "../../../../common/hooks/useDocumentFocus";
import { ReportAnIssueModal } from "./modals/ReportAnIssueModal";
import { RedactionLogModal } from "./redactionLog/RedactionLogModal";
export const path = "/case-details/:urn/:id";

type Props = BackLinkingPageProps & {};

export const Page: React.FC<Props> = ({ backLinkProps }) => {
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
    handleCloseErrorModal,
    handleUnLockDocuments,
    handleShowHideDocumentIssueModal,
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tabsState.items.length]);

  if (caseState.status === "loading") {
    // if we are waiting on the main case details call, show holding message
    //  (we are prepared to show page whilst waiting for docs to load though)
    return <WaitPage />;
  }

  const isMultipleDefendantsOrCharges = isMultipleChargeCase(caseState.data);

  const dacDocumentId = getDACDocumentId(
    pipelineState?.haveData ? pipelineState.data.documents : []
  );

  const getActiveTabDocument = () => {
    return tabsState.items.find(
      (item) => item.documentId === tabsState.activeTabId
    );
  };

  console.log(
    "tabsState>>>redactionHighlights",
    getActiveTabDocument()?.redactionHighlights
  );

  console.log("RedactionLogData", redactionLog.redactionLogData);

  return (
    <>
      {errorModal.show && (
        <Modal
          isVisible
          handleClose={handleCloseErrorModal}
          type="alert"
          ariaLabel="Error Modal"
          ariaDescription={errorModal.title}
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
          documentId={getActiveTabDocument()?.documentId!}
          presentationTitle={getActiveTabDocument()?.presentationTitle!}
          polarisDocumentVersionId={
            getActiveTabDocument()?.polarisDocumentVersionId!
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
        redactionLog.redactionLogData.status === "succeeded" && (
          <RedactionLogModal
            redactionHighlights={getActiveTabDocument()!.redactionHighlights}
            savingStatus={getActiveTabDocument()!.savingStatus}
            redactionLogData={redactionLog.redactionLogData.data}
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
          <div
            role="region"
            aria-labelledby="side-panel-region-label"
            id="side-panel"
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
                  accordionState={accordionState.data}
                  handleOpenPdf={(caseDoc) => {
                    handleOpenPdf({ ...caseDoc, mode: "read" });
                  }}
                />
              )}
            </div>
          </div>
          <div
            className={`govuk-grid-column-three-quarters ${classes.rightColumn}`}
          >
            {!tabsState.items.length ? (
              <PdfTabsEmpty pipelineState={pipelineState} />
            ) : (
              <PdfTabs
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
                contextData={{
                  correlationId: pipelineState?.correlationId,
                }}
              />
            )}
          </div>
        </div>
      </PageContentWrapper>
    </>
  );
};

export default Page;
