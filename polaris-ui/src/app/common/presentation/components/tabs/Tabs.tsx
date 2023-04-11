import { useEffect, useState, useRef } from "react";
import { CommonTabsProps } from "./types";
import { useLastFocus } from "../../../hooks/useLastFocus";
import { Modal } from "../../../../common/presentation/components/Modal";
import { NavigationAwayAlertContent } from "../../../../features/cases/presentation/case-details/navigation-alerts/NavigationAwayAlertContent";
import { ReactComponent as CloseIcon } from "../../svgs/closeIconBold.svg";
import classes from "./Tabs.module.scss";

const ARROW_KEY_SHIFTS = {
  ArrowLeft: -1,
  ArrowUp: -1,
  ArrowRight: 1,
  ArrowDown: 1,
};

export type TabsProps = CommonTabsProps & {
  activeTabId: string | undefined;
  handleTabSelection: (documentId: string) => void;
  handleClosePdf: (caseDocument: { documentId: string }) => void;
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
  ...attributes
}) => {
  const activeTabRef = useRef<HTMLButtonElement>(null);
  const [showDocumentNavAlert, setShowDocumentNavAlert] = useState(false);

  useEffect(() => {
    activeTabRef.current?.focus();
  }, [activeTabId, items.length]);
  useLastFocus(document.querySelector("#case-details-search") as HTMLElement);

  const activeTabArrayPos = items.findIndex((item) => item.id === activeTabId);
  const activeTabIndex = activeTabArrayPos === -1 ? 0 : activeTabArrayPos;

  const handleKeyPressOnTab: React.KeyboardEventHandler<HTMLButtonElement> = (
    ev
  ) => {
    const typedKeyCode = ev.code as keyof typeof ARROW_KEY_SHIFTS;
    const thisShift = ARROW_KEY_SHIFTS[typedKeyCode]; // -1, 1 or undefined
    const shouldNavigate =
      // must be a left or right key press command
      !!thisShift &&
      // can't go left on the first tab
      !(activeTabIndex === 0 && thisShift === -1) &&
      // can't go right on the last tab
      !(activeTabIndex === items.length - 1 && thisShift === 1);

    if (!shouldNavigate) {
      return;
    }

    const nextTabIndex = activeTabIndex + ARROW_KEY_SHIFTS[typedKeyCode];
    const nextTabId = items[nextTabIndex].id;
    handleTabSelection(nextTabId);

    // prevent awkward vertical scroll on up/down key press
    ev.preventDefault();
  };

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

  const handleNavigateAwayContinueAction = () => {
    setShowDocumentNavAlert(false);
    localHandleClosePdf();
  };

  const renderTabs = () => {
    if (!items.length) {
      return null;
    }
    return (
      <div className={`${classes.tabsWrapper}`}>
        <ul className={`${classes.tabsList}   perma-scrollbar`} role="tablist">
          {items.map((item, index) => {
            const { id: itemId, label } = item;

            return (
              <li
                className={`${
                  activeTabIndex === index
                    ? classes.activeTab
                    : classes.inactiveTab
                } ${classes.tabListItem}`}
                key={itemId}
                data-testid={`tab-${index}`}
                role="presentation"
              >
                <button
                  id={`tab_${index}`}
                  aria-controls={`panel-${index}`}
                  role="tab"
                  className={classes.tabButton}
                  data-testid={
                    index === activeTabIndex ? "tab-active" : `btn-tab-${index}`
                  }
                  onClick={() => handleTabSelection(itemId)}
                  onKeyDown={handleKeyPressOnTab}
                  ref={index === activeTabIndex ? activeTabRef : undefined}
                  tabIndex={index === activeTabIndex ? 0 : -1}
                >
                  <span className={classes.tabLabel}> {label}</span>
                </button>

                {activeTabIndex === index && (
                  <button
                    className={classes.tabCloseButton}
                    onClick={handleCloseTab}
                    onKeyDown={handleKeyPressOnTab}
                    data-testid="tab-remove"
                  >
                    <CloseIcon />
                  </button>
                )}
              </li>
            );
          })}
        </ul>
      </div>
    );
  };

  const panels = items.map((item, index) => {
    const { id: itemId, panel } = item;
    const panelId = itemId;

    const coreProps = {
      key: panelId,
      role: "tabpanel",
      tabIndex: 0,
      "aria-labelledby": `tab_${index}`,
      "data-testid": "tab-content-" + itemId,
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

  return (
    <>
      <div
        id={id}
        data-testid="tabs"
        className={`govuk-tabs ${classes.tabs} ${className || ""}`}
        {...attributes}
      >
        {renderTabs()}
        {panels}
      </div>

      {showDocumentNavAlert && (
        <Modal
          isVisible
          handleClose={handleNavigateAwayCancelAction}
          type={"alert"}
        >
          <NavigationAwayAlertContent
            handleCancelAction={handleNavigateAwayCancelAction}
            handleContinueAction={handleNavigateAwayContinueAction}
          />
        </Modal>
      )}
    </>
  );
};
