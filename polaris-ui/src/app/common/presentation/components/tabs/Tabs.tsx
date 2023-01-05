import { useEffect, useRef } from "react";
import { useHistory, useLocation } from "react-router-dom";
import { CommonTabsProps } from "./types";

import classes from "./Tabs.module.scss";

const ARROW_KEY_SHIFTS = {
  ArrowLeft: -1,
  ArrowUp: -1,
  ArrowRight: 1,
  ArrowDown: 1,
};

const getIdFromHash = (hash: string) => hash.replace("#", "");

// try and stay as close to GDS...
export type TabsProps = CommonTabsProps & {
  // ...but we extend to allow the tabs to have a close icon
  handleClosePdf: (caseDocument: { tabSafeId: string }) => void;
};

export const Tabs: React.FC<TabsProps> = ({
  className,
  id,
  idPrefix,
  items,
  title,
  handleClosePdf,
  ...attributes
}) => {
  const history = useHistory();
  const activeTabRef = useRef<HTMLAnchorElement>(null);
  const { hash } = useLocation();

  useEffect(() => {
    activeTabRef.current?.focus();
  }, [hash]);

  const activeTabArrayPos = items.findIndex(
    (item) => item.id === getIdFromHash(hash)
  );
  const activeTabIndex = activeTabArrayPos === -1 ? 0 : activeTabArrayPos;

  const handleKeyPressOnTab: React.KeyboardEventHandler<HTMLAnchorElement> = (
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
    history.push(`#${nextTabId}`);

    // prevent awkward vertical scroll on up/down key press
    ev.preventDefault();
  };

  const localHandleClosePdf = (id: string) => {
    const thisItemIndex = items.findIndex((item) => item.id === id);
    const nextTabIndex =
      items.length === 1
        ? undefined // there is only item so next item is empty
        : thisItemIndex === 0
        ? 1 // we are removing the first item, so we need the item to the right
        : thisItemIndex - 1; // otherwise, we need the item to the left

    const nextTabId = nextTabIndex === undefined ? "" : items[nextTabIndex].id;
    history.push(`#${nextTabId}`);
    handleClosePdf({ tabSafeId: id });
  };

  const closeIcon = (
    <svg>
      <circle
        cx="12"
        cy="12"
        r="13"
        stroke="white"
        strokeWidth="1"
        fill="white"
      ></circle>
      <path
        stroke="white"
        strokeWidth="3"
        fill="none"
        d="M6.25,6.25,17.75,17.75"
      ></path>
      <path
        stroke="white"
        strokeWidth="3"
        fill="none"
        d="M6.25,17.75,17.75,6.25"
      ></path>
    </svg>
  );

  const tabContent = items.map((item, index) => {
    const { id: itemId, label, panel, ...itemAttributes } = item;
    const tabId = itemId;

    const coreHyperlinkProps = {
      className: "govuk-tabs__tab",
      role: "tab",
      href: `#${tabId}`,
      "aria-controls": String(index),
      id: `tab_${index}`,
      ...itemAttributes,
    };

    return index === activeTabIndex ? (
      <li
        key={tabId}
        className="govuk-tabs__list-item govuk-tabs__list-item--selected"
        role="presentation"
      >
        <a
          {...coreHyperlinkProps}
          data-testid="tab-active"
          tabIndex={0}
          aria-selected="true"
          onKeyDown={handleKeyPressOnTab}
          ref={activeTabRef}
        >
          {label}
        </a>
        <span>
          <button
            onClick={() => localHandleClosePdf(item.id)}
            data-testid="tab-remove"
          >
            {closeIcon}
          </button>
        </span>
      </li>
    ) : (
      <li key={tabId} className="govuk-tabs__list-item" role="presentation">
        <a {...coreHyperlinkProps} tabIndex={-1} aria-selected="false">
          <span>{label}</span>
        </a>
      </li>
    );
  });

  const tabs =
    items.length > 0 ? (
      <ul className="govuk-tabs__list perma-scrollbar" role="tablist">
        {tabContent}
      </ul>
    ) : null;

  const panels = items.map((item, index) => {
    const { id: itemId, panel } = item;
    const panelId = itemId;

    const coreProps = {
      key: panelId,
      role: "tabpanel",
      "aria-labelledby": `tab_${index}`,
      "data-testid": "tab-content-" + itemId,
      //disable hash navigation scrolling.  If we uncomment the below (as per standard GDS)
      // and give the panel an id, the screen will jump on every tab navigation
      // id: panelId,
    };

    return index === activeTabIndex ? (
      <div {...coreProps} className="govuk-tabs__panel">
        {panel.children}
      </div>
    ) : (
      <div
        {...coreProps}
        className="govuk-tabs__panel govuk-tabs__panel--hidden"
      >
        {panel.children}
      </div>
    );
  });

  return (
    <div
      id={id}
      data-testid="tabs"
      className={`govuk-tabs ${classes.tabs} ${className || ""}`}
      {...attributes}
      // data-module="govuk-tabs"
      // ref={tabsRef}
    >
      <h2 className="govuk-tabs__title" data-testid="txt-tabs-title">
        {title}
      </h2>
      {tabs}
      {panels}
    </div>
  );
};
