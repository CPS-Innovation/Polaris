import { useMemo } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { REPORT_ISSUE } from "../../../../../config";
import { DropdownButton } from "../../../../../common/presentation/components";
import { isAlreadyReportedDocument } from "../../../../../common/utils/reportDocuments";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./HeaderReadMode.module.scss";

type Props = {
  showOverRedactionLog: boolean;
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "read" }>;
  handleShowHideDocumentIssueModal: CaseDetailsState["handleShowHideDocumentIssueModal"];
  handleShowRedactionLogModal: CaseDetailsState["handleShowRedactionLogModal"];
  contextData: {
    documentId: string;
    tabIndex: number;
  };
};

export const HeaderReadMode: React.FC<Props> = ({
  showOverRedactionLog,
  caseDocumentViewModel: { sasUrl },
  handleShowHideDocumentIssueModal,
  handleShowRedactionLogModal,
  contextData,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const disableReportBtn = isAlreadyReportedDocument(contextData.documentId);

  const handleDocumentAction = (id: string) => {
    switch (id) {
      case "1":
        handleShowRedactionLogModal("over");
        trackEvent("Open Under Over Redaction Log", { documentId: id });
        break;
      case "2":
        handleShowHideDocumentIssueModal(true);
        break;
    }
  };

  const dropDownItems = useMemo(() => {
    let items = [
      {
        id: "2",
        label: disableReportBtn ? "Issue reported" : "Report an issue",
        ariaLabel: "report an issue",
        disabled: disableReportBtn,
      },
    ];
    if (showOverRedactionLog) {
      items = [
        {
          id: "1",
          label: "Log an Under/Over redaction",
          ariaLabel: "log an under or over redaction",
          disabled: false,
        },
        ...items,
      ];
    }
    return items;
  }, [showOverRedactionLog, disableReportBtn]);

  return (
    <div className={classes.content}>
      <DropdownButton
        currentSelectionId={dropDownItems[0].id}
        dropDownItems={dropDownItems}
        callBackFn={handleDocumentAction}
        ariaLabel="document actions dropdown"
        dataTestId="document-actions-dropdown"
      />
    </div>
  );
};
