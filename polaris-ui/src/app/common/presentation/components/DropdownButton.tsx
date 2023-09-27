import { useState, useEffect, useRef, useCallback } from "react";
import { ReactComponent as DownArrow } from "../svgs/down.svg";
import { LinkButton } from "../components/LinkButton";
import { useFocusTrap } from "../../hooks/useFocusTrap";
import classes from "./DropdownButton.module.scss";
export type DropdownButtonProps = {
  dropDownItems: { id: string; label: string }[];
  callBackFn: (id: string) => void;
  ariaLabel?:string;
};

export const DropdownButton: React.FC<DropdownButtonProps> = ({
  dropDownItems,
  callBackFn,
  ariaLabel="dropdown"
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
        ref={dropDownBtnRef}
        ariaLabel={buttonOpen?`${ariaLabel} close`:`${ariaLabel} open`}
        ariaExpanded={buttonOpen}
        className={`${classes.dropDownButton} ${buttonOpen && classes.upArrow}`}
        disabled={dropDownItems.length < 2}
        onClick={() => {
          setButtonOpen((buttonOpen) => !buttonOpen);
        }}
      >
        <DownArrow />
      </LinkButton>

      {buttonOpen && (
        <div className={classes.panel} ref={panelRef} id="dropdown-panel">
          <ul className={classes.tabList}>
            {dropDownItems.map((item) => (
              <li className={classes.tabListItem}>
                <LinkButton
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
