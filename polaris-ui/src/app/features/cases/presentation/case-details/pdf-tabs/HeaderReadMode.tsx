import { useEffect } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { FeedbackButton } from "../../../../../common/presentation/components/feedback/FeedbackButton";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { REPORT_ISSUE } from "../../../../../config";
import classes from "./HeaderReadMode.module.scss";

type Props = {
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "read" }>;
  handleShowHideDocumentIssueModal: CaseDetailsState["handleShowHideDocumentIssueModal"];
  contextData: {
    documentId: string;
    tabIndex: number;
  };
};

export const HeaderReadMode: React.FC<Props> = ({
  caseDocumentViewModel: { sasUrl },
  handleShowHideDocumentIssueModal,
  contextData,
}) => {
  useEffect(() => {
    if (sasUrl) {
      window.open(sasUrl, "_blank")?.focus();
    }
  }, [sasUrl]);

  return (
    <div className={classes.content}>
      {REPORT_ISSUE && (
        <FeedbackButton
          {...contextData}
          handleShowHideDocumentIssueModal={handleShowHideDocumentIssueModal}
        />
      )}
    </div>
  );
};
