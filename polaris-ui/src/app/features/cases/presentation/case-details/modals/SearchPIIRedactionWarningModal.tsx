import { useState, useEffect, useRef } from "react";
import {
  Button,
  LinkButton,
  Checkboxes,
  Modal,
  ErrorSummary,
} from "../../../../../common/presentation/components/index";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./SearchPIIRedactionWarningModal.module.scss";

type Props = {
  documentId: string;
  documentType: string;
  versionId?: number;
  acceptedAllSearchPIIRedactionsCount: number;
  hideRedactionWarningModal: () => void;
  handleContinue: () => void;
};

export const SearchPIIRedactionWarningModal: React.FC<Props> = ({
  documentId,
  documentType,
  versionId,
  acceptedAllSearchPIIRedactionsCount,
  hideRedactionWarningModal,
  handleContinue,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [userConfirmation, setUserConfirmation] = useState(false);
  const [userConfirmationError, setUserConfirmationError] = useState(false);
  const errorSummaryRef = useRef(null);
  useEffect(() => {
    if (userConfirmationError && errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [userConfirmationError]);

  const handleContinueButtonClick = () => {
    if (!userConfirmation) {
      setUserConfirmationError(true);
      if (userConfirmationError && errorSummaryRef.current) {
        (errorSummaryRef?.current as HTMLButtonElement).focus();
      }
      return;
    }
    if (userConfirmationError) setUserConfirmationError(false);
    handleContinue();
  };
  const handleClosePIIRedactionWarningModal = () => {
    trackEvent("Cancel Save Redaction Suggestion Warning", {
      documentId,
      documentType,
      versionId,
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
        defaultLastFocusId="#active-tab-panel"
      >
        <div className={classes.modalHeader}>
          <h2>{`Confirm redaction suggestions`}</h2>
        </div>
        <div className={classes.contentWrapper}>
          {userConfirmationError && (
            <div
              ref={errorSummaryRef}
              tabIndex={-1}
              className={classes.errorSummaryWrapper}
            >
              <ErrorSummary
                data-testid={"warning-error-summary"}
                className={classes.errorSummary}
                errorList={[
                  {
                    reactListKey: "1",
                    children: `Please confirm you have reviewed the whole document and the redactions to be applied are intended.`,
                    href: "#terms-and-condition",
                    "data-testid": "terms-and-condition-link",
                  },
                ]}
              />
            </div>
          )}
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
                userConfirmationError
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
