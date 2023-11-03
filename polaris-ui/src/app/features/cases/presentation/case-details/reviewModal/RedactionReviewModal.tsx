import { Modal } from "../../../../../common/presentation/components";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { Button } from "../../../../../common/presentation/components/Button";
import { Table } from "govuk-react-jsx";
import { ReactComponent as DeleteIcon } from "../../../../../common/presentation/svgs/delete.svg";
import classes from "./RedactionReviewModal.module.scss";
type Props = {
  //   // This is intentionally narrower than ApiResult<...> as we definitely have
  //   //  the Api result and means we do not have to check for the "loading" case
  //   //  from here onwards.
  //   caseState: SucceededApiResult<CaseDetails>;
  //   searchTerm: CaseDetailsState["searchTerm"];
  //   searchState: CaseDetailsState["searchState"];
  //   pipelineState: CaseDetailsState["pipelineState"];
  //   handleSearchTermChange: CaseDetailsState["handleSearchTermChange"];
  //   handleCloseSearchResults: CaseDetailsState["handleCloseSearchResults"];
  //   handleLaunchSearchResults: CaseDetailsState["handleLaunchSearchResults"];
  //   handleChangeResultsOrder: CaseDetailsState["handleChangeResultsOrder"];
  //   handleUpdateFilter: CaseDetailsState["handleUpdateFilter"];
  //   handleOpenPdf: CaseDetailsState["handleOpenPdf"];
  tabsState: CaseDetailsState["tabsState"];

  handleReviewRedactions: (value: boolean) => void;
  handleRemoveRedaction: (documentId: string, redactionId: string) => void;
};

export const RedactionReviewModal: React.FC<Props> = ({
  handleReviewRedactions,
  tabsState,
  handleRemoveRedaction,
}) => {
  const getActiveTabItem = () =>
    tabsState.items.find((item) => item.documentId === tabsState.activeTabId);

  const handleCloseModal = () => {
    handleReviewRedactions(false);
  };
  // const handleRemoveRedaction = (id: string) => {
  //   console.log("handle remove redaction::", id);
  // };

  const renderUnsavedRedactions = () => {
    const tabItem = getActiveTabItem();

    console.log("unsavedRedactions>>", tabItem);
    const rows = tabItem?.redactionHighlights?.reduce((acc, current, index) => {
      console.log("highlight image>>", current);
      acc.push({
        cells: [
          {
            children: (
              <h3 className={classes.tableCell}>{`Redaction ${index + 1}`}</h3>
            ),
          },
          {
            children: (
              <div className={classes.tableCell}>
                {current.image ? (
                  <div
                    className={classes.imageContainer}
                    style={{ marginTop: "0.5rem" }}
                  >
                    <img
                      className={classes.image}
                      src={current.image}
                      alt={"Screenshot"}
                    />
                  </div>
                ) : null}
              </div>
            ),
          },
          {
            children: (
              <div className={classes.tableCell}>
                <button
                  className={classes.deleteButton}
                  onClick={() =>
                    handleRemoveRedaction(tabItem?.documentId, current.id)
                  }
                >
                  <DeleteIcon />
                </button>
              </div>
            ),
          },
        ],
      });
      return acc;
    }, [] as any);

    return <Table head={[]} rows={rows} />;
  };
  return (
    <Modal
      isVisible={true}
      handleClose={handleCloseModal}
      ariaLabel="Search Modal"
      ariaDescription="Find your search results"
    >
      <div className={classes.modalHeader}>
        <h1>{`"${getActiveTabItem()?.presentationFileName}" redactions`}</h1>
      </div>
      <div className={classes.modalSubHeader}>
        <h2>Review your redactions</h2>
      </div>
      <div className={classes.contentWrapper}>
        <div className={classes.tableWrapper}>{renderUnsavedRedactions()}</div>
      </div>
    </Modal>
  );
};
