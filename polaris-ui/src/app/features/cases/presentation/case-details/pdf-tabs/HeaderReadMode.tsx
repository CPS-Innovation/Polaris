import { useEffect } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { REPORT_ISSUE } from "../../../../../config";
import { DropdownButton } from "../../../../../common/presentation/components";
import { isAlreadyReportedDocument } from "../../../../../common/utils/reportDocuments";
import classes from "./HeaderReadMode.module.scss";

type Props = {
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "read" }>;
  handleShowHideDocumentIssueModal: CaseDetailsState["handleShowHideDocumentIssueModal"];
  handleShowRedactionLogModal: CaseDetailsState["handleShowRedactionLogModal"];
  contextData: {
    documentId: string;
    tabIndex: number;
  };
};

export const HeaderReadMode: React.FC<Props> = ({
  caseDocumentViewModel: { sasUrl },
  handleShowHideDocumentIssueModal,
  handleShowRedactionLogModal,
  contextData,
}) => {
  const disableReportBtn = isAlreadyReportedDocument(contextData.documentId);
  useEffect(() => {
    if (sasUrl) {
      window.open(sasUrl, "_blank")?.focus();
    }
  }, [sasUrl]);

  const handleDocumentAction = (id: string) => {
    console.log("document action>>", id);
    switch (id) {
      case "1":
        handleShowRedactionLogModal("over");
        break;
      case "2":
        handleShowHideDocumentIssueModal(true);
        break;
    }
  };

  return (
    <div className={classes.content}>
      <DropdownButton
        currentSelectionId={"1"}
        dropDownItems={[
          {
            id: "1",
            label: "Log an Under/Over redaction",
            ariaLabel: "log an under or over redaction",
            disabled: false,
          },
          {
            id: "2",
            label: disableReportBtn ? "Issue reported" : "Report an issue",
            ariaLabel: "report an issue",
            disabled: disableReportBtn,
          },
        ]}
        callBackFn={handleDocumentAction}
        ariaLabel="document actions dropdown"
        dataTestId="document-actions-dropdown"
      />
    </div>
  );
};
