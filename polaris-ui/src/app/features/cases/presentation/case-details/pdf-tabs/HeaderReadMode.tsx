import { useEffect, useState } from "react";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { Button } from "../../../../../common/presentation/components/Button";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import {
  isAlreadyReportedDocument,
  addToReportedDocuments,
} from "../utils/reportDocuments";
import classes from "./HeaderReadMode.module.scss";

type Props = {
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "read" }>;
  handleOpenPdfInNewTab: (
    documentId: CaseDocumentViewModel["documentId"]
  ) => void;
  handleIssueReporting: (
    documentId: CaseDocumentViewModel["documentId"]
  ) => void;
};

export const HeaderReadMode: React.FC<Props> = ({
  caseDocumentViewModel: { presentationFileName, sasUrl, documentId },
  handleOpenPdfInNewTab,
  handleIssueReporting,
}) => {
  const [disableReportBtn, setDisableReportBtn] = useState(
    isAlreadyReportedDocument(documentId)
  );
  const trackEvent = useAppInsightsTrackEvent();
  useEffect(() => {
    if (sasUrl) {
      window.open(sasUrl, "_blank");
    }
  }, [sasUrl]);

  console.log("disabled>>>>", disableReportBtn);
  return (
    <div className={classes.content}>
      <LinkButton
        dataTestId="btn-open-pdf"
        onClick={() => {
          trackEvent("Open Document In Tab", {
            documentId: documentId,
            presentationFileName: presentationFileName,
          });
          handleOpenPdfInNewTab(documentId);
        }}
      >
        {presentationFileName} (opens in a new window)
      </LinkButton>

      <Button
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
        Report an issue
      </Button>
    </div>
  );
};
