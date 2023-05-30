import { useParams, useHistory } from "react-router-dom";
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
import { ConfirmationModalContent } from "../../../../common/presentation/components/ConfirmationModalContent";
import { SURVEY_LINK } from "../../../../config";
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
    confirmationModal,
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
    handleOpenPdfInNewTab,
    handleCloseErrorModal,
    handleUnLockDocuments,
  } = useCaseDetailsState(urn, +caseId);

  const {
    showAlert,
    setShowAlert,
    newPath,
    navigationUnblockHandle,
    unSavedRedactionDocs,
  } = useNavigationAlert(tabsState.items);

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
        <Modal isVisible handleClose={handleCloseErrorModal} type="alert">
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
              handleTabSelection(params.documentId);
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
            handleSearchTermChange,
            handleLaunchSearchResults,
            handleCloseSearchResults,
            handleChangeResultsOrder,
            handleUpdateFilter,
            handleOpenPdf: (caseDoc) => {
              handleOpenPdf(caseDoc);
              handleTabSelection(caseDoc.documentId);
            },
          }}
        />
      )}
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
        onClick={() => trackEvent("Back To Find A Case")}
      >
        {backLinkProps.label}
      </BackLink>

      <PageContentWrapper>
        <div className={`govuk-grid-row ${classes.mainContent}`}>
          <div
            className={`govuk-grid-column-one-quarter perma-scrollbar ${classes.leftColumn}`}
          >
            <div>
              <KeyDetails
                handleOpenPdf={handleOpenPdf}
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
                trackEventKey="Search Case Documents From Case File"
              />

              {accordionState.status === "loading" ? (
                <AccordionWait />
              ) : (
                <Accordion
                  accordionState={accordionState.data}
                  handleOpenPdf={(caseDoc) => {
                    handleOpenPdf({ ...caseDoc, mode: "read" });
                    handleTabSelection(caseDoc.documentId);
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
                pipelineState={pipelineState}
                tabsState={tabsState}
                savedDocumentDetails={pipelineRefreshData.savedDocumentDetails}
                handleTabSelection={handleTabSelection}
                handleClosePdf={handleClosePdf}
                handleLaunchSearchResults={handleLaunchSearchResults}
                handleAddRedaction={handleAddRedaction}
                handleRemoveRedaction={handleRemoveRedaction}
                handleRemoveAllRedactions={handleRemoveAllRedactions}
                handleSavedRedactions={handleSavedRedactions}
                handleOpenPdfInNewTab={handleOpenPdfInNewTab}
                handleUnLockDocuments={handleUnLockDocuments}
                feedbackData={{
                  correlationId: pipelineState?.correlationId,
                  urn: urn,
                  caseId: caseId,
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
