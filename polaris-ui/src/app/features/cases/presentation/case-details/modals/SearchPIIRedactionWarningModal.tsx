import { useState } from "react";
import {
  Button,
  LinkButton,
  Checkboxes,
  Modal,
} from "../../../../../common/presentation/components/index";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./SearchPIIRedactionWarningModal.module.scss";

type Props = {
  documentId: string;
  documentType: string;
  polarisDocumentVersionId?: number;
  acceptedSearchPIIRedactionsCount: number;
  hideRedactionWarningModal: () => void;
  handleContinue: () => void;
};

export const SearchPIIRedactionWarningModal: React.FC<Props> = ({
  documentId,
  documentType,
  polarisDocumentVersionId,
  acceptedSearchPIIRedactionsCount,
  hideRedactionWarningModal,
  handleContinue,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
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
  const handleClosePIIRedactionWarningModal = () => {
    trackEvent("Cancel Save Redaction Suggestion Warning", {
      documentId: documentId,
      documentType: documentType,
      polarisDocumentVersionId: polarisDocumentVersionId,
      suggestedRedactionsCount: acceptedSearchPIIRedactionsCount,
    });
    hideRedactionWarningModal();
  };
  const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setUserConfirmation(e.target.checked);
  };

  return (
    <>
      <Modal
        isVisible={true}
        handleClose={handleClosePIIRedactionWarningModal}
        className={classes.redactionWarningModal}
        ariaLabel="Use potential redactions confirmation Modal"
        ariaDescription={`Your remaining ${acceptedSearchPIIRedactionsCount} potential redactions will also be redacted, if you choose to continue`}
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
              {`Your remaining ${acceptedSearchPIIRedactionsCount} potential redactions will also be redacted, if
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
              name="terms-and-condition"
              data-testid="terms-and-condition"
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
              onClick={handleClosePIIRedactionWarningModal}
            >
              Cancel
            </LinkButton>
          </div>
        </div>
      </Modal>
    </>
  );
};
