import { useEffect } from "react";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
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
  caseDocumentViewModel: { presentationFileName, sasUrl, documentId },
  handleOpenPdfInNewTab,
  contextData,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  useEffect(() => {
    if (sasUrl) {
      window.open(sasUrl, "_blank")?.focus();
    }
  }, [sasUrl]);

  return (
    <div className={classes.content}>
      <LinkButton
        id="btn-open-pdf"
        dataTestId="btn-open-pdf"
        className={classes.btnOpenPdf}
        onClick={() => {
          trackEvent("Open Document In Tab", {
            documentId: documentId,
          });
          // handleOpenPdfInNewTab(documentId);
        }}
      >
        {presentationFileName} (opens in a new tab)
      </LinkButton>
      {REPORT_ISSUE && <FeedbackButton {...contextData} />}
    </div>
  );
};
