import { useRef } from "react";
import { Modal } from "../../../../../common/presentation/components";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { Table } from "govuk-react-jsx";
import { ReactComponent as DeleteIcon } from "../../../../../common/presentation/svgs/delete.svg";
import classes from "./RedactionReviewModal.module.scss";
type Props = {
  tabsState: CaseDetailsState["tabsState"];
  handleReviewRedactions: (value: boolean) => void;
  handleRemoveRedaction: (documentId: string, redactionId: string) => void;
};

export const RedactionReviewModal: React.FC<Props> = ({
  handleReviewRedactions,
  tabsState,
  handleRemoveRedaction,
}) => {
  const closeBtnRef = useRef<HTMLButtonElement>(null);
  const getActiveTabItem = () =>
    tabsState.items.find((item) => item.documentId === tabsState.activeTabId);

  const getSubHeading = () => {
    if (!tabItem.redactionHighlights.length) {
      return `You have ${tabItem.redactionHighlights.length} unsaved redactions`;
    }
    if (tabItem.redactionHighlights.length === 1) {
      return `You have ${tabItem.redactionHighlights.length} unsaved redaction,
      review your redaction`;
    }
    return `You have ${tabItem.redactionHighlights.length} unsaved redactions,
    review your redactions`;
  };

  const handleCloseModal = () => {
    handleReviewRedactions(false);
  };

  const tabItem = getActiveTabItem()!;

  const renderUnsavedRedactions = () => {
    const rows = tabItem?.redactionHighlights?.reduce((acc, current, index) => {
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
                      alt={
                        current.highlightType === "area"
                          ? `Screenshot of area redaction, the redacted content is unavailable to read`
                          : `Screenshot of text redaction, the redacted text is ${current.textContent}`
                      }
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
                  aria-label={
                    current.highlightType === "area"
                      ? `Delete redaction button, redacted content is unavailable to read`
                      : `Delete redaction button, the redacted text is ${current.textContent}`
                  }
                  onClick={() => {
                    handleRemoveRedaction(tabItem?.documentId, current.id);
                    if (closeBtnRef.current) {
                      closeBtnRef.current?.focus();
                    }
                  }}
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
      forwardedRef={closeBtnRef}
    >
      <div className={classes.modalHeader}>
        <h1>{`"${tabItem?.presentationFileName}" redactions`}</h1>
      </div>
      <div className={classes.modalSubHeader}>
        <h2 aria-live="polite">{getSubHeading()}</h2>
      </div>
      <div className={classes.contentWrapper}>
        {!!tabItem?.redactionHighlights.length && (
          <div className={classes.tableWrapper}>
            {renderUnsavedRedactions()}
          </div>
        )}
      </div>
    </Modal>
  );
};
