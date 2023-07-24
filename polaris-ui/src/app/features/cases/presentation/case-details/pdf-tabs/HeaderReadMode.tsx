import { useEffect } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import {
  FeedbackButton,
  FeedbackButtonProps,
} from "../../../../../common/presentation/components/feedback/FeedbackButton";
import { REPORT_ISSUE } from "../../../../../config";
import classes from "./HeaderReadMode.module.scss";

type Props = {
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "read" }>;
  handleOpenPdfInNewTab: (
    documentId: CaseDocumentViewModel["documentId"]
  ) => void;
  contextData: FeedbackButtonProps;
};

export const HeaderReadMode: React.FC<Props> = ({
  caseDocumentViewModel: { sasUrl },
  handleOpenPdfInNewTab,
  contextData,
}) => {
  useEffect(() => {
    if (sasUrl) {
      window.open(sasUrl, "_blank")?.focus();
    }
  }, [sasUrl]);

  return (
    <div className={classes.content}>
      {REPORT_ISSUE && <FeedbackButton {...contextData} />}
    </div>
  );
};
