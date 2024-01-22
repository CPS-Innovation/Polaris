import { useState, useEffect, useRef, useCallback } from "react";
import { ReactComponent as DownArrow } from "../svgs/down.svg";
import { LinkButton } from "../components/LinkButton";
import { useFocusTrap } from "../../hooks/useFocusTrap";
import classes from "./DropdownButton.module.scss";

export type DropdownButtonItem = {
  id: string;
  label: string;
  ariaLabel: string;
  disabled: boolean;
};
export type DropdownButtonProps = {
  name?: string;
  dropDownItems: DropdownButtonItem[];
  callBackFn: (id: string) => void;
  ariaLabel?: string;
  dataTestId?: string;
  disabled?: boolean;
  showLastItemSeparator?: boolean;
};

export const DropdownButton: React.FC<DropdownButtonProps> = ({
  dropDownItems,
  callBackFn,
  name,
  dataTestId = "dropdown-btn",
  ariaLabel = "dropdown",
  disabled = false,
  showLastItemSeparator = false,
}) => {
  const dropDownBtnRef = useRef<HTMLButtonElement | null>(null);
  const panelRef = useRef<HTMLDivElement | null>(null);
  const [buttonOpen, setButtonOpen] = useState(false);
  const buttonOpenRef = useRef<boolean>(false);
  useFocusTrap("#dropdown-panel");

  const handleBtnClick = (id: string) => {
    setButtonOpen(false);
    callBackFn(id);
  };

  useEffect(() => {
    buttonOpenRef.current = buttonOpen;
  }, [buttonOpen]);

  const handleOutsideClick = useCallback((event: MouseEvent) => {
    if (panelRef.current && event.target && buttonOpenRef.current) {
      if (!panelRef.current?.contains(event.target as Node)) {
        setButtonOpen(false);
        event.stopPropagation();
      }
    }
  }, []);

  const keyDownHandler = useCallback((event: KeyboardEvent) => {
    if (event.code === "Escape" && buttonOpenRef.current) {
      setButtonOpen(false);
      dropDownBtnRef.current?.focus();
    }
  }, []);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    document.addEventListener("click", handleOutsideClick);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
      document.removeEventListener("click", handleOutsideClick);
    };
  }, []);

  return (
    <div className={classes.dropDownButtonWrapper}>
      <LinkButton
        id={dataTestId}
        dataTestId={dataTestId}
        ref={dropDownBtnRef}
        ariaLabel={ariaLabel}
        ariaExpanded={buttonOpen}
        className={`${classes.dropDownButton} ${
          buttonOpen && classes.upArrow
        } ${name && classes.btnWithText}`}
        disabled={disabled}
        onClick={() => {
          setButtonOpen((buttonOpen) => !buttonOpen);
        }}
      >
        {name && <span className={classes.dropdownBtnName}>{name}</span>}
        <DownArrow />
      </LinkButton>

      {buttonOpen && (
        <div
          className={classes.panel}
          ref={panelRef}
          id="dropdown-panel"
          data-testid={`dropdown-panel`}
        >
          <ul
            className={
              showLastItemSeparator
                ? `${classes.tabList} ${classes.tabListWithSeparator}`
                : `${classes.tabList}`
            }
          >
            {dropDownItems.map((item) => (
              <li key={item.id} className={classes.tabListItem}>
                <LinkButton
                  ariaLabel={item.ariaLabel}
                  disabled={item.disabled}
                  onClick={() => {
                    handleBtnClick(item.id);
                  }}
                >
                  {item.label}
                </LinkButton>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};
