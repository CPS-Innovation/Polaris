import { useMemo, useCallback } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { REPORT_ISSUE } from "../../../../../config";
import {
  DropdownButton,
  DropdownButtonItem,
} from "../../../../../common/presentation/components/DropdownButton";
import {
  LinkButton,
  Tooltip,
} from "../../../../../common/presentation/components";
import { isAlreadyReportedDocument } from "../../../../../common/utils/reportDocuments";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { RedactionLogTypes } from "../../../domain/redactionLog/RedactionLogTypes";
import { ReactComponent as AreaIcon } from "../../../../../common/presentation/svgs/areaIcon.svg";
import classes from "./HeaderReadMode.module.scss";

type Props = {
  showOverRedactionLog: boolean;
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "read" }>;
  handleShowHideDocumentIssueModal: CaseDetailsState["handleShowHideDocumentIssueModal"];
  handleShowRedactionLogModal: CaseDetailsState["handleShowRedactionLogModal"];
  handleAreaOnlyRedaction: CaseDetailsState["handleAreaOnlyRedaction"];
  handleShowHideRedactionSuggestions: CaseDetailsState["handleShowHideRedactionSuggestions"];
  contextData: {
    documentId: string;
    tabIndex: number;
    areaOnlyRedactionMode: boolean;
    searchPIIOn: string[];
    showSearchPII: boolean;
  };
};

export const HeaderReadMode: React.FC<Props> = ({
  showOverRedactionLog,
  caseDocumentViewModel: { sasUrl },
  handleShowHideDocumentIssueModal,
  handleShowRedactionLogModal,
  handleAreaOnlyRedaction,
  handleShowHideRedactionSuggestions,
  contextData,
}) => {
  console.log("searchPIIOn>>", contextData.searchPIIOn);
  console.log("contextData.documentId>>", contextData.documentId);

  const trackEvent = useAppInsightsTrackEvent();
  const disableReportBtn = isAlreadyReportedDocument(contextData.documentId);

  const handleDocumentAction = (id: string) => {
    switch (id) {
      case "1":
        handleShowRedactionLogModal(RedactionLogTypes.UNDER_OVER);
        trackEvent("Open Under Over Redaction Log", { documentId: id });
        break;
      case "2":
        handleShowHideDocumentIssueModal(true);
        break;
      case "3":
        handleShowHideRedactionSuggestions(contextData.documentId, !isPIIOn);
    }
  };

  const isPIIOn = useMemo(() => {
    return contextData.searchPIIOn.includes(contextData.documentId);
  }, [contextData.searchPIIOn, contextData.documentId]);

  const dropDownItems = useMemo(() => {
    let items: DropdownButtonItem[] = [];
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
    if (REPORT_ISSUE) {
      items = [
        ...items,
        {
          id: "2",
          label: disableReportBtn ? "Issue reported" : "Report an issue",
          ariaLabel: "report an issue",
          disabled: disableReportBtn,
        },
      ];
    }
    if (contextData.showSearchPII) {
      items = [
        ...items,
        {
          id: "3",
          label: isPIIOn
            ? "Turn off potential redactions"
            : "Turn on potential redactions",
          ariaLabel: isPIIOn
            ? "Turn off potential redactions"
            : "Turn on potential redactions",
          disabled: false,
        },
      ];
    }

    return items;
  }, [showOverRedactionLog, disableReportBtn, isPIIOn]);

  const handleRedactAreaToolButtonClick = useCallback(() => {
    if (window.getSelection()) {
      window.getSelection()?.removeAllRanges();
    }
    handleAreaOnlyRedaction(
      contextData.documentId,
      !contextData.areaOnlyRedactionMode
    );
    if (!contextData.areaOnlyRedactionMode) {
      trackEvent("Redact Area Tool On", {
        documentId: contextData.documentId,
      });
      return;
    }
    trackEvent("Redact Area Tool Off", {
      documentId: contextData.documentId,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    contextData.areaOnlyRedactionMode,
    contextData.documentId,
    handleAreaOnlyRedaction,
  ]);

  return (
    <div className={classes.content}>
      <Tooltip
        text={
          contextData.areaOnlyRedactionMode
            ? "Redact area tool on"
            : "Redact area tool off"
        }
      >
        <LinkButton
          className={
            contextData.areaOnlyRedactionMode
              ? `${classes.areaToolBtn} ${classes.areaToolBtnEnabled}`
              : classes.areaToolBtn
          }
          dataTestId={`btn-area-tool-${contextData.tabIndex}`}
          id={`btn-area-tool-${contextData.tabIndex}`}
          ariaLabel={
            !contextData.areaOnlyRedactionMode
              ? "enable area redaction mode"
              : "disable area redaction mode"
          }
          onClick={handleRedactAreaToolButtonClick}
        >
          <AreaIcon />
        </LinkButton>
      </Tooltip>
      <DropdownButton
        name="Document actions"
        dropDownItems={dropDownItems}
        callBackFn={handleDocumentAction}
        ariaLabel="document actions dropdown"
        dataTestId={`document-actions-dropdown-${contextData.tabIndex}`}
        showLastItemSeparator={true}
      />
    </div>
  );
};
