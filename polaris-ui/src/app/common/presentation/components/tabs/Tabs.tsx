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
  handleClosePdf: (caseDocument: { documentId: string }) => void;
  handleUnLockDocuments: (documentIds: string[]) => void;
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
  ...attributes
}) => {
  const [showDocumentNavAlert, setShowDocumentNavAlert] = useState(false);

  useLastFocus(document.querySelector("#case-details-search") as HTMLElement);

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
    handleClosePdf({ documentId: items[activeTabIndex].id });
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
      "aria-labelledby": `tab_${index}`,
      "data-testid": `tab-content-${itemId}`,
      //disable hash navigation scrolling.  If we uncomment the below (as per standard GDS)
      // and give the panel an id, the screen will jump on every tab navigation
      id: `panel-${index}`,
    };

    return (
      <div
        {...coreProps}
        className={`govuk-tabs__panel ${
          index !== activeTabIndex ? "govuk-tabs__panel--hidden" : ""
        }`}
      >
        {panel.children}
      </div>
    );
  });

  const tabItems = items.map((item) => ({
    id: item.id,
    label: item.label,
  }));

  return (
    <>
      <div
        id={id}
        data-testid="tabs"
        className={`govuk-tabs ${classes.tabs} ${className || ""}`}
        {...attributes}
      >
        <TabButtons
          items={tabItems}
          activeTabIndex={activeTabIndex}
          handleTabSelection={handleTabSelection}
          handleCloseTab={handleCloseTab}
          handleUnLockDocuments={handleUnLockDocuments}
        />
        {panels}
      </div>

      {showDocumentNavAlert && (
        <Modal
          isVisible
          handleClose={handleNavigateAwayCancelAction}
          type={"alert"}
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
