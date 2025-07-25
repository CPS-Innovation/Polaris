import { useState } from "react";
import { CommonTabsProps } from "./types";
import { useLastFocus } from "../../../hooks/useLastFocus";
import { Modal } from "../../../../common/presentation/components/Modal";
import { NavigationAwayAlertContent } from "../../../../features/cases/presentation/case-details/navigation-alerts/NavigationAwayAlertContent";
import TabButtons from "./TabButtons";
import classes from "./Tabs.module.scss";

export type TabsProps = CommonTabsProps & {
  activeTabId: string | undefined;
  handleTabSelection: (documentId: string) => void;
  handleClosePdf: (documentId: string, versionId: number) => void;
  handleUnLockDocuments: (documentIds: string[]) => void;
  dcfMode: string | undefined;
};

export const Tabs: React.FC<TabsProps> = ({
  className,
  id,
  idPrefix,
  items,
  title,
  activeTabId,
  handleTabSelection,
  handleClosePdf,
  handleUnLockDocuments,
  dcfMode,
  ...attributes
}) => {
  const [showDocumentNavAlert, setShowDocumentNavAlert] = useState(false);

  useLastFocus("#case-details-search");

  const activeTabArrayPos = items.findIndex((item) => item.id === activeTabId);
  const activeTabIndex = activeTabArrayPos === -1 ? 0 : activeTabArrayPos;

  const handleCloseTab = () => {
    const { isDirty } = items[activeTabIndex];
    if (isDirty) {
      setShowDocumentNavAlert(true);
      return;
    }
    localHandleClosePdf();
  };

  const localHandleClosePdf = () => {
    const thisItemIndex = activeTabIndex;
    const nextTabIndex =
      items.length === 1
        ? undefined // there is only item so next item is empty
        : thisItemIndex === 0
        ? 1 // we are removing the first item, so we need the item to the right
        : thisItemIndex - 1; // otherwise, we need the item to the left

    const nextTabId = nextTabIndex === undefined ? "" : items[nextTabIndex].id;
    handleTabSelection(nextTabId);
    handleClosePdf(items[activeTabIndex].id, items[activeTabIndex].versionId);
  };

  const handleNavigateAwayCancelAction = () => {
    setShowDocumentNavAlert(false);
  };

  const handleNavigateAwayContinueAction = (documentIds: string[]) => {
    setShowDocumentNavAlert(false);
    localHandleClosePdf();
    handleUnLockDocuments(documentIds);
  };

  const panels = items.map((item, index) => {
    const { id: itemId, panel } = item;
    const panelId = itemId;

    const coreProps = {
      key: panelId,
      role: "tabpanel",
      tabIndex: 0,
      "data-testid": `tab-content-${itemId}`,
    };

    return (
      <div
        id={index === activeTabIndex ? "active-tab-panel" : `panel-${index}`}
        aria-labelledby={
          index === activeTabIndex
            ? "document-panel-region-label"
            : `tab_${index}`
        }
        {...coreProps}
        className={`govuk-tabs__panel ${
          index !== activeTabIndex ? classes.hideTabDocument : ""
        }  ${classes.contentArea}`}
      >
        {index === activeTabIndex && (
          <span
            id="document-panel-region-label"
            className={classes.tabPanelRegionLabel}
          >
            Document view port
          </span>
        )}
        {panel.children}
      </div>
    );
  });

  const tabItems = items.map((item) => ({
    id: item.id,
    label: item.label,
    ariaLabel: `Document ${item.label}`,
  }));

  return (
    <>
      <div
        data-testid="tabs"
        className={`govuk-tabs ${classes.tabs} ${className || ""} `}
        {...attributes}
      >
        <TabButtons
          items={tabItems}
          activeTabIndex={activeTabIndex}
          handleTabSelection={handleTabSelection}
          handleCloseTab={handleCloseTab}
          handleUnLockDocuments={handleUnLockDocuments}
          dcfMode={dcfMode}
        />
        {panels}
      </div>

      {showDocumentNavAlert && (
        <Modal
          isVisible
          handleClose={handleNavigateAwayCancelAction}
          type={"alert"}
          ariaLabel="Unsaved redaction warning modal"
          ariaDescription="You are navigating away from document with unsaved redaction"
        >
          <NavigationAwayAlertContent
            type="document"
            documentId={activeTabId!}
            handleCancelAction={handleNavigateAwayCancelAction}
            handleContinueAction={handleNavigateAwayContinueAction}
          />
        </Modal>
      )}
    </>
  );
};
