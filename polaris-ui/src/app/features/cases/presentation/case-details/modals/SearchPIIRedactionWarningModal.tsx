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
  acceptedAllSearchPIIRedactionsCount: number;
  hideRedactionWarningModal: () => void;
  handleContinue: () => void;
};

export const SearchPIIRedactionWarningModal: React.FC<Props> = ({
  documentId,
  documentType,
  polarisDocumentVersionId,
  acceptedAllSearchPIIRedactionsCount,
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
      acceptedAllRedactionsCount: acceptedAllSearchPIIRedactionsCount,
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
        ariaLabel="Confirm redaction suggestions Modal"
        ariaDescription={`You have chosen to 'accept all' for ${acceptedAllSearchPIIRedactionsCount} redaction suggestions. If you choose to continue, redactions will be applied which you may not have reviewed individually`}
        defaultLastFocus={
          document.querySelector("#active-tab-panel") as HTMLElement
        }
      >
        <div className={classes.modalHeader}>
          <h2>{`Confirm redaction suggestions`}</h2>
        </div>
        <div className={classes.contentWrapper}>
          <div className={classes.mainText}>
            <span className="govuk-warning-text__icon" aria-hidden="true">
              !
            </span>
            <p className={classes.contentText}>
              {`You have chosen to 'accept all' for ${acceptedAllSearchPIIRedactionsCount} redaction suggestions. If you choose to continue, redactions will be applied which you may not have reviewed individually.`}
            </p>
          </div>
          <div>
            <Checkboxes
              onChange={handleCheckboxChange}
              errorMessage={
                error
                  ? {
                      children:
                        "Please confirm you have reviewed the whole document and the redactions to be applied are intended.",
                    }
                  : undefined
              }
              items={[
                {
                  children:
                    "I have reviewed the whole document and confirm the redactions to be applied are intended.",
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
