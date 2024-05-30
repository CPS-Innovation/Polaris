import { useState, useMemo } from "react";
import {
  Button,
  LinkButton,
  Checkboxes,
  Modal,
} from "../../../../../common/presentation/components/index";
import { ISearchPIIHighlight } from "../../../domain/NewPdfHighlight";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./SearchPIIRedactionWarningModal.module.scss";

type Props = {
  documentId: string;
  polarisDocumentVersionId?: number;
  activeSearchPIIHighlights: ISearchPIIHighlight[];
  hideRedactionWarningModal: () => void;
  handleContinue: () => void;
};

export const SearchPIIRedactionWarningModal: React.FC<Props> = ({
  documentId,
  polarisDocumentVersionId,
  activeSearchPIIHighlights,
  hideRedactionWarningModal,
  handleContinue,
}) => {
  const suggestedRedactionCount = useMemo(() => {
    return activeSearchPIIHighlights.filter(
      (highlight) => highlight.redactionStatus === "redacted"
    ).length;
  }, [activeSearchPIIHighlights]);
  const [userConfirmation, setUserConfirmation] = useState(false);
  const [error, setError] = useState(false);

  const handleContinueButtonClick = () => {
    if (!userConfirmation) {
      setError(true);
      return;
    }
    if (error) setError(false);
    handleContinue();
  };
  const handleIIRedactionWarningModalClose = () => {
    hideRedactionWarningModal();
  };
  const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setUserConfirmation(e.target.checked);
  };

  return (
    <>
      <Modal
        isVisible={true}
        handleClose={handleIIRedactionWarningModalClose}
        className={classes.redactionWarningModal}
        ariaLabel="Use potential redactions confirmation Modal"
        ariaDescription={`Your remaining ${suggestedRedactionCount} potential redactions will also be redacted, if you choose to continue`}
        defaultLastFocus={
          document.querySelector("#active-tab-panel") as HTMLElement
        }
      >
        <div className={classes.modalHeader}>
          <h2>{`Use potential redactions?`}</h2>
        </div>
        <div className={classes.contentWrapper}>
          <div className={classes.mainText}>
            <span className="govuk-warning-text__icon" aria-hidden="true">
              !
            </span>
            <p className={classes.contentText}>
              {`Your remaining ${suggestedRedactionCount} potential redactions will also be redacted, if
              you choose to continue`}
            </p>
          </div>
          <div>
            <Checkboxes
              onChange={handleCheckboxChange}
              errorMessage={
                error
                  ? {
                      children:
                        "Please accept you have manually checked all selected redactions in the document",
                    }
                  : undefined
              }
              items={[
                {
                  children:
                    "I have manually checked all selected redactions in the document",
                  value: "yes",
                },
              ]}
              name="t-and-c"
            />
          </div>

          <div className={classes.actionWrapper}>
            <Button
              className={classes.continueBtn}
              onClick={handleContinueButtonClick}
              data-testid="btn-continue"
            >
              Continue
            </Button>
            <LinkButton
              dataTestId="btn-cancel"
              onClick={handleIIRedactionWarningModalClose}
            >
              Cancel
            </LinkButton>
          </div>
        </div>
      </Modal>
    </>
  );
};
