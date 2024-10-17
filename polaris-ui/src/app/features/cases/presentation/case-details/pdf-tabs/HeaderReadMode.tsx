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
  handleShowHidePageRotation: CaseDetailsState["handleShowHidePageRotation"];
  handleShowHidePageDeletion: CaseDetailsState["handleShowHidePageDeletion"];
  handleShowHideRedactionSuggestions: (
    documentId: string,
    showSuggestion: boolean,
    defaultOption: boolean
  ) => void;
  contextData: {
    documentId: string;
    tabIndex: number;
    areaOnlyRedactionMode: boolean;
    isSearchPIIOn: boolean;
    isSearchPIIDefaultOptionOn: boolean;
    showSearchPII: boolean;
    rotatePageMode: boolean;
    deletePageMode: boolean;
    showRotatePage: boolean;
    showDeletePage: boolean;
  };
};

export const HeaderReadMode: React.FC<Props> = ({
  showOverRedactionLog,
  caseDocumentViewModel: { sasUrl },
  handleShowHideDocumentIssueModal,
  handleShowRedactionLogModal,
  handleAreaOnlyRedaction,
  handleShowHideRedactionSuggestions,
  handleShowHidePageRotation,
  handleShowHidePageDeletion,
  contextData,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const disableReportBtn = isAlreadyReportedDocument(contextData.documentId);

  const handleDocumentAction = (id: string) => {
    switch (id) {
      case "1":
        handleShowRedactionLogModal(RedactionLogTypes.UNDER_OVER);
        trackEvent("Open Under Over Redaction Log", {
          documentId: contextData.documentId,
        });
        break;
      case "2":
        handleShowHideDocumentIssueModal(true);
        break;
      case "3":
        handleShowHideRedactionSuggestions(
          contextData.documentId,
          !contextData.isSearchPIIOn,
          true
        );
        break;
      case "4":
        handleShowHideRedactionSuggestions(
          contextData.documentId,
          !contextData.isSearchPIIOn,
          false
        );
        break;
      case "5":
        handleShowHidePageRotation(
          contextData.documentId,
          !contextData.rotatePageMode
        );
        break;
      case "6":
        handleShowHidePageDeletion(
          contextData.documentId,
          !contextData.deletePageMode
        );
        break;
    }
  };

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
          label: contextData.isSearchPIIOn
            ? "Turn off potential redactions 1"
            : "Turn on potential redactions 1",
          ariaLabel: contextData.isSearchPIIOn
            ? "Turn off potential redactions 1"
            : "Turn on potential redactions 1",
          disabled:
            contextData.isSearchPIIOn &&
            !contextData.isSearchPIIDefaultOptionOn,
        },
        {
          id: "4",
          label: contextData.isSearchPIIOn
            ? "Turn off potential redactions 2"
            : "Turn on potential redactions 2",
          ariaLabel: contextData.isSearchPIIOn
            ? "Turn off potential redactions 2"
            : "Turn on potential redactions 2",
          disabled:
            contextData.isSearchPIIOn && contextData.isSearchPIIDefaultOptionOn,
        },
      ];
    }

    if (contextData.showRotatePage) {
      items = [
        ...items,
        {
          id: "5",
          label: contextData.rotatePageMode
            ? "Hide Rotate Page Options"
            : "Show Rotate Page Options",
          ariaLabel: contextData.rotatePageMode
            ? "Hide Rotate Page Options"
            : "Show Rotate Page Options",
          disabled: false,
        },
      ];
    }
    if (contextData.showDeletePage) {
      items = [
        ...items,
        {
          id: "6",
          label: contextData.deletePageMode
            ? "Hide Delete Page Options"
            : "Show Delete Page Options",
          ariaLabel: contextData.deletePageMode
            ? "Hide Delete Page Options"
            : "Show Delete Page Options",
          disabled: false,
        },
      ];
    }
    return items;
  }, [
    showOverRedactionLog,
    disableReportBtn,
    contextData.isSearchPIIOn,
    contextData.showSearchPII,
    contextData.isSearchPIIDefaultOptionOn,
    contextData.showRotatePage,
    contextData.rotatePageMode,
    contextData.showDeletePage,
    contextData.deletePageMode,
  ]);

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
            ? "Redact area tool Off"
            : "Redact area tool On"
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
            contextData.areaOnlyRedactionMode
              ? "disable area redaction mode"
              : "enable area redaction mode"
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
