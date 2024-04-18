import { useState } from "react";
import {
  Button,
  LinkButton,
  TextArea,
  Modal,
} from "../../../../../common/presentation/components/index";
import { ConfirmationModalContent } from "../../../../../common/presentation/components/feedback/ConfirmationModalContent";
import { addToReportedDocuments } from "../../../../../common/utils/reportDocuments";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./ReportAnIssueModal.module.scss";

type Props = {
  documentId: string;
  documentTypeId: number;
  correlationId: string;
  polarisDocumentVersionId: number;
  presentationTitle: string;

  handleShowHideDocumentIssueModal: (value: boolean) => void;
};

export const ReportAnIssueModal: React.FC<Props> = ({
  documentId,
  documentTypeId,
  presentationTitle,
  correlationId,
  polarisDocumentVersionId,
  handleShowHideDocumentIssueModal,
}) => {
  const [issueDescription, setIssueDescription] = useState("");
  const [showConfirmationModal, setShowConfirmationModal] = useState(false);
  const trackEvent = useAppInsightsTrackEvent();

  const handleIssueReporting = () => {
    trackEvent("Report Document Issue", {
      documentId: documentId,
      documentTypeId: documentTypeId,
      polarisVersionId: correlationId,
      correlationId: polarisDocumentVersionId,
      fileName: presentationTitle,
      moreDetails: issueDescription,
    });
    setShowConfirmationModal(true);
    addToReportedDocuments(documentId);
  };
  const handleDocumentIssueModalClose = () => {
    handleShowHideDocumentIssueModal(false);
  };

  const handleConfirmationModalClose = () => {
    setShowConfirmationModal(false);
    handleShowHideDocumentIssueModal(false);
  };
  const handleTextAreaChange = (
    event: React.ChangeEvent<HTMLTextAreaElement>
  ) => {
    setIssueDescription(event.target?.value);
  };
  return (
    <>
      {!showConfirmationModal && (
        <Modal
          isVisible={true}
          handleClose={handleDocumentIssueModalClose}
          className={classes.reportIssueModal}
          ariaLabel="Report an Issue Modal"
          ariaDescription={`Report a problem with the document "${presentationTitle}"`}
          defaultLastFocus={
            document.querySelector("#active-tab-panel") as HTMLElement
          }
        >
          <div className={classes.modalHeader}>
            <h2>{`Report a problem with: "${presentationTitle}"`}</h2>
          </div>
          <div className={classes.contentWrapper}>
            <div>
              <TextArea
                name="more-details"
                id="more-details"
                data-testid="report-issue-more-details"
                hint={{
                  children: (
                    <span>
                      Don't include personal or sensitive information about the
                      case.
                    </span>
                  ),
                }}
                label={{
                  children: (
                    <span className={classes.textAreaLabel}>
                      Tell us what went wrong with this document
                    </span>
                  ),
                }}
                onChange={handleTextAreaChange}
                value={issueDescription}
              />
            </div>

            <div className={classes.actionWrapper}>
              <Button
                disabled={!issueDescription}
                className={classes.saveBtn}
                onClick={handleIssueReporting}
                data-testid="btn-report-issue-save"
              >
                Save and return
              </Button>
              <LinkButton
                dataTestId="btn-report-issue-close"
                onClick={handleDocumentIssueModalClose}
              >
                Close
              </LinkButton>
            </div>
          </div>
        </Modal>
      )}

      {showConfirmationModal && (
        <Modal
          isVisible={true}
          handleClose={handleConfirmationModalClose}
          type="alert"
          ariaLabel="Confirmation Modal"
          ariaDescription="Thanks for reporting an issue with this document"
        >
          <ConfirmationModalContent
            message="Thanks for reporting an issue with this document."
            handleClose={handleConfirmationModalClose}
          />
        </Modal>
      )}
    </>
  );
};
