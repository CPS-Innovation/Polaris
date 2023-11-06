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
  correlationId: string;
  polarisDocumentVersionId: number;
  presentationTitle: string;

  handleShowHideDocumentIssueModal: (value: boolean) => void;
};

export const ReportAnIssueModal: React.FC<Props> = ({
  documentId,
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
      polarisVersionId: correlationId,
      correlationId: polarisDocumentVersionId,
      description: issueDescription,
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
          ariaDescription="Report a problem with this document"
        >
          <div className={classes.modalHeader}>
            <h1>{`Report a problem with: "${presentationTitle}"`}</h1>
          </div>
          <div className={classes.contentWrapper}>
            <div className={classes.subHeaderWrapper}>
              <h3 className={classes.modalSubHeader}>
                Tell us what went wrong with this document
              </h3>
              <span>
                Don't include personal or sensitive information about the case
              </span>
            </div>

            <div>
              <TextArea
                onChange={handleTextAreaChange}
                value={issueDescription}
              />
            </div>

            <div className={classes.actionWrapper}>
              <Button
                disabled={!issueDescription}
                className={classes.saveBtn}
                onClick={handleIssueReporting}
              >
                Save and return
              </Button>
              <LinkButton onClick={handleDocumentIssueModalClose}>
                Close
              </LinkButton>
            </div>
          </div>
        </Modal>
      )}
      {showConfirmationModal && (
        <Modal
          isVisible={showConfirmationModal}
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
