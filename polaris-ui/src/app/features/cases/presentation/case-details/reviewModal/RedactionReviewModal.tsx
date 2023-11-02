import { Modal } from "../../../../../common/presentation/components";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { Button } from "../../../../../common/presentation/components/Button";
import { Table } from "govuk-react-jsx";
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
  const handleCloseModal = () => {
    handleReviewRedactions(false);
  };
  // const handleRemoveRedaction = (id: string) => {
  //   console.log("handle remove redaction::", id);
  // };

  const renderUnsavedRedactions = () => {
    const tabItem = tabsState.items.find(
      (item) => item.documentId === tabsState.activeTabId
    );

    console.log("unsavedRedactions>>", tabItem?.documentId);
    const rows = tabItem?.redactionHighlights?.reduce((acc, current, index) => {
      acc.push({
        cells: [
          { children: <span>{`${index + 1}`}</span> },
          { children: <span>{current.textContent}</span> },
          {
            children: (
              <Button
                className={classes.errorOkBtn}
                onClick={() =>
                  handleRemoveRedaction(tabItem?.documentId, current.id)
                }
              >
                remove redaction
              </Button>
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
      <h2>Review unsaved redactions</h2>
      <div className={classes.contentWrapper}>
        <div className={classes.tableWrapper}>{renderUnsavedRedactions()}</div>
      </div>
    </Modal>
  );
};
