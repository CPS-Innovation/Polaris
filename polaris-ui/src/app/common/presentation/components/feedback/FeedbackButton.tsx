import { Button } from "../Button";
import {
  isAlreadyReportedDocument,
  addToReportedDocuments,
} from "../../../utils/reportDocuments";
import { useAppInsightsTrackEvent } from "../../../hooks/useAppInsightsTracks";
import { Modal } from "../Modal";
import { ConfirmationModalContent } from "./ConfirmationModalContent";
import { useState } from "react";
import classes from "./FeedbackButton.module.scss";
export type FeedbackButtonProps = {
  documentId: string;
  correlationId: string;
  polarisDocumentVersionId: number;
  tabIndex: number;
};
export const FeedbackButton: React.FC<FeedbackButtonProps> = ({
  documentId,
  correlationId,
  polarisDocumentVersionId,
  tabIndex,
}) => {
  const trackEvent = useAppInsightsTrackEvent();

  const [disableReportBtn, setDisableReportBtn] = useState(
    isAlreadyReportedDocument(documentId)
  );
  const [showConfirmationModal, setShowConfirmationModal] = useState(false);

  const handleIssueReporting = (documentId: string) => {
    trackEvent("Report Document Issue", {
      documentId: documentId,
      polarisVersionId: polarisDocumentVersionId,
      correlationId: correlationId,
    });

    setShowConfirmationModal(true);
  };
  return (
    <>
      <div className={`${classes.content}`}>
        <Button
          id={`btn-report-issue-${tabIndex}`}
          name="secondary"
          className={`${classes.btnReportIssue} govuk-button--secondary`}
          disabled={disableReportBtn}
          onClick={() => {
            setDisableReportBtn(true);
            addToReportedDocuments(documentId);
            handleIssueReporting(documentId);
          }}
          data-testid="btn-report-issue"
        >
          {disableReportBtn ? "Issue reported" : "Report an issue"}
        </Button>
      </div>
      {showConfirmationModal && (
        <Modal
          isVisible={showConfirmationModal}
          handleClose={() => setShowConfirmationModal(false)}
          type="alert"
          ariaLabel="Confirmation Modal"
          ariaDescription="Thanks for reporting an issue with this document"
        >
          <ConfirmationModalContent
            message="Thanks for reporting an issue with this document."
            handleClose={() => setShowConfirmationModal(false)}
          />
        </Modal>
      )}
    </>
  );
};
