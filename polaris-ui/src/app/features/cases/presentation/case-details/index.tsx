import { useParams, useHistory } from "react-router-dom";
import { BackLink } from "../../../../common/presentation/components";
import { PageContentWrapper } from "../../../../common/presentation/components";
import { WaitPage } from "../../../../common/presentation/components";
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
import { checkMultipleDefendantsOrCharges } from "./utils/checkMultipleDefendantsOrCharges";
export const path = "/case-details/:urn/:id";

type Props = BackLinkingPageProps & {};

export const Page: React.FC<Props> = ({ backLinkProps }) => {
  const history = useHistory();
  const { id, urn } = useParams<{ id: string; urn: string }>();

  const {
    caseState,
    accordionState,
    tabsState,
    searchState,
    searchTerm,
    pipelineState,
    handleOpenPdf,
    handleClosePdf,
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
  } = useCaseDetailsState(urn, +id);

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

  const isMultipleDefendantsOrCharges = checkMultipleDefendantsOrCharges(
    caseState.data
  );

  return (
    <>
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
            handleContinueAction={() => {
              setShowAlert(false);
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
            handleSearchTermChange,
            handleLaunchSearchResults,
            handleCloseSearchResults,
            handleChangeResultsOrder,
            handleUpdateFilter,
            handleOpenPdf,
          }}
        />
      )}

      <BackLink to={backLinkProps.to}>{backLinkProps.label}</BackLink>

      <PageContentWrapper>
        <div className={`govuk-grid-row ${classes.mainContent}`}>
          <div
            className={`govuk-grid-column-one-quarter perma-scrollbar ${classes.leftColumn}`}
          >
            <div>
              <KeyDetails
                caseDetails={caseState.data}
                isMultipleDefendantsOrCharges={isMultipleDefendantsOrCharges}
              />

              {!isMultipleDefendantsOrCharges && (
                <Charges caseDetails={caseState.data} />
              )}

              <SearchBox
                data-testid="search-case"
                labelText="Search"
                value={searchTerm}
                handleChange={handleSearchTermChange}
                handleSubmit={handleLaunchSearchResults}
              />

              {accordionState.status === "loading" ? (
                <AccordionWait />
              ) : (
                <Accordion
                  accordionState={accordionState.data}
                  handleOpenPdf={(caseDoc) =>
                    handleOpenPdf({ ...caseDoc, mode: "read" })
                  }
                />
              )}
            </div>
          </div>
          <div
            className={`govuk-grid-column-three-quarters ${classes.rightColumn}`}
          >
            {!tabsState.items.length ? (
              <PdfTabsEmpty />
            ) : (
              <PdfTabs
                tabsState={tabsState}
                handleClosePdf={handleClosePdf}
                handleLaunchSearchResults={handleLaunchSearchResults}
                handleAddRedaction={handleAddRedaction}
                handleRemoveRedaction={handleRemoveRedaction}
                handleRemoveAllRedactions={handleRemoveAllRedactions}
                handleSavedRedactions={handleSavedRedactions}
                handleOpenPdfInNewTab={handleOpenPdfInNewTab}
              />
            )}
          </div>
        </div>
      </PageContentWrapper>
    </>
  );
};

export default Page;
