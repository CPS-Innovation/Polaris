import { useEffect, useRef, useMemo } from "react";
import classes from "./Tabs.module.scss";
import { ReactComponent as CloseIcon } from "../../svgs/closeIconBold.svg";
import { ReactComponent as DownArrow } from "../../svgs/down.svg";
import { LinkButton } from "../../../../common/presentation/components/LinkButton";
import { DropdownButton } from "../../../../common/presentation/components";

export type TabButtonProps = {
  items: { id: string; label: string; ariaLabel: string }[];
  activeTabIndex: number;
  handleTabSelection: (documentId: string) => void;
  handleCloseTab: () => void;
  handleUnLockDocuments: (documentIds: string[]) => void;
  dcfMode: string | undefined;
};

const ARROW_KEY_SHIFTS = {
  ArrowLeft: -1,
  ArrowRight: 1,
};

const TabButtons: React.FC<TabButtonProps> = ({
  items,
  activeTabIndex,
  handleTabSelection,
  handleCloseTab,
  dcfMode,
}) => {
  const activeTabRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    activeTabRef.current?.focus();
    activeTabRef.current?.parentElement?.scrollIntoView({
      behavior: "smooth",
      block: "nearest",
    });
  }, [activeTabIndex, items.length]);

  const handleKeyPressOnTab: React.KeyboardEventHandler<HTMLButtonElement> = (
    ev
  ) => {
    const typedKeyCode = ev.code as keyof typeof ARROW_KEY_SHIFTS;
    const thisShift = ARROW_KEY_SHIFTS[typedKeyCode]; // -1, 1 or undefined
    if (!thisShift) {
      return;
    }
    moveToNextOrPreviousTab(thisShift);
    if (ev.code === "ArrowRight" || ev.code === "ArrowLeft") {
      ev.preventDefault();
    }
  };

  const moveToNextOrPreviousTab = (thisShift: number) => {
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

    const nextTabIndex = activeTabIndex + thisShift;
    const nextTabId = items[nextTabIndex].id;
    handleTabSelection(nextTabId);
  };

  const tabDropdownItems = useMemo(() => {
    return items.map((item) => ({
      ...item,
      disabled: item.id === items[activeTabIndex].id,
    }));
  }, [items, activeTabIndex]);

  if (!items.length) {
    return null;
  }
  return (
    <div
      role="region"
      aria-labelledby="document-tabs-region-label"
      // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
      tabIndex={0}
      id="document-tabs"
      className={`${classes.tabsWrapper} ${classes.contentArea}`}
    >
      <span id="document-tabs-region-label" className={classes.tabsRegionLabel}>
        Document control area
      </span>
      <div className={`${classes.arrowBtnsWrapper}`}>
        <LinkButton
          disabled={activeTabIndex === 0}
          className={classes.tabPreviousButton}
          dataTestId="btn-tab-previous"
          ariaLabel="tab previous"
          onClick={() => {
            moveToNextOrPreviousTab(-1);
          }}
        >
          <DownArrow />
        </LinkButton>
        <LinkButton
          disabled={activeTabIndex === items.length - 1}
          className={classes.tabNextButton}
          dataTestId="btn-tab-next"
          ariaLabel="tab next"
          onClick={() => {
            moveToNextOrPreviousTab(1);
          }}
        >
          <DownArrow />
        </LinkButton>
      </div>

      <ul className={`${classes.tabsList}`} role="tablist">
        {items.map((item, index) => {
          const { id: itemId, label, ariaLabel } = item;

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
                aria-controls={
                  index === activeTabIndex
                    ? "active-tab-panel"
                    : `panel-${index}`
                }
                aria-label={ariaLabel}
                role="tab"
                className={classes.tabButton}
                data-testid={
                  index === activeTabIndex ? "tab-active" : `btn-tab-${index}`
                }
                onClick={() => {
                  if (itemId !== items[activeTabIndex].id) {
                    handleTabSelection(itemId);
                  }
                }}
                onKeyDown={handleKeyPressOnTab}
                tabIndex={index === activeTabIndex ? 0 : -1}
                ref={index === activeTabIndex ? activeTabRef : undefined}
              >
                <span className={classes.tabLabel}>{label}</span>
              </button>
              {activeTabIndex === index && !dcfMode && (
                <button
                  role="tab"
                  className={classes.tabCloseButton}
                  onClick={handleCloseTab}
                  onKeyDown={handleKeyPressOnTab}
                  data-testid="tab-remove"
                  aria-label="close tab"
                >
                  <CloseIcon />
                </button>
              )}
            </li>
          );
        })}
      </ul>
      <div className={classes.tabsDropDownWrapper}>
        <DropdownButton
          dropDownItems={tabDropdownItems}
          callBackFn={handleTabSelection}
          ariaLabel="tabs dropdown"
          dataTestId="tabs-dropdown"
          disabled={items.length < 2}
        />
      </div>
    </div>
  );
};

export default TabButtons;
