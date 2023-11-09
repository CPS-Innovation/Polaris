import { Button } from "../Button";
import { isAlreadyReportedDocument } from "../../../utils/reportDocuments";
import classes from "./FeedbackButton.module.scss";
export type FeedbackButtonProps = {
  documentId: string;
  tabIndex: number;
  handleShowHideDocumentIssueModal: (value: boolean) => void;
};
export const FeedbackButton: React.FC<FeedbackButtonProps> = ({
  documentId,
  tabIndex,
  handleShowHideDocumentIssueModal,
}) => {
  const disableReportBtn = isAlreadyReportedDocument(documentId);

  return (
    <div className={`${classes.content}`}>
      <Button
        id={`btn-report-issue-${tabIndex}`}
        name="secondary"
        className={`${classes.btnReportIssue} govuk-button--secondary`}
        disabled={disableReportBtn}
        onClick={() => {
          handleShowHideDocumentIssueModal(true);
        }}
        data-testid={`btn-report-issue-${tabIndex}`}
      >
        {disableReportBtn ? "Issue reported" : "Report an issue"}
      </Button>
    </div>
  );
};
