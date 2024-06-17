import { useState, useEffect, useRef } from "react";
import {
  Button,
  LinkButton,
  Checkboxes,
  Modal,
  ErrorSummary,
} from "../../../../../common/presentation/components/index";
import { ISearchPIIHighlight } from "../../../domain/NewPdfHighlight";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./SearchPIIRedactionWarningModal.module.scss";

type Props = {
  documentId: string;
  documentType: string;
  polarisDocumentVersionId?: number;
  activeSearchPIIHighlights: ISearchPIIHighlight[];
  hideRedactionWarningModal: () => void;
  handleContinue: () => void;
};

export const SearchPIIRedactionWarningModal: React.FC<Props> = ({
  documentId,
  documentType,
  polarisDocumentVersionId,
  activeSearchPIIHighlights,
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
      documentId: documentId,
      documentType: documentType,
      polarisDocumentVersionId: polarisDocumentVersionId,
      suggestedRedactionsCount: activeSearchPIIHighlights.length,
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
        ariaDescription={`Your remaining ${activeSearchPIIHighlights.length} potential redactions will also be redacted, if you choose to continue`}
        defaultLastFocus={
          document.querySelector("#active-tab-panel") as HTMLElement
        }
      >
        <div className={classes.modalHeader}>
          <h2>{`Use potential redactions?`}</h2>
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
                    children: `Please accept you have manually checked all selected redactions in the document`,
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
              {`Your remaining ${activeSearchPIIHighlights.length} potential redactions will also be redacted, if
              you choose to continue`}
            </p>
          </div>
          <div>
            <Checkboxes
              onChange={handleCheckboxChange}
              errorMessage={
                userConfirmationError
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
