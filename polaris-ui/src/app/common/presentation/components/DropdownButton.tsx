import { useState, useEffect, useRef, useCallback } from "react";
import { ReactComponent as DownArrow } from "../svgs/down.svg";
import { LinkButton } from "../components/LinkButton";
import classes from "./DropdownButton.module.scss";
export type DropdownButtonProps = {
  dropDownItems: { id: string; label: string }[];
  callBackFn: (id: string) => void;
};

export const DropdownButton: React.FC<DropdownButtonProps> = ({
  dropDownItems,
  callBackFn,
}) => {
  const panelRef = useRef<HTMLDivElement | null>(null);
  const [buttonOpen, setButtonOpen] = useState(false);
  const buttonOpenRef = useRef<boolean>(false);

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

  useEffect(() => {
    document.addEventListener("click", handleOutsideClick);
    return () => {
      document.removeEventListener("click", handleOutsideClick);
    };
  }, []);

  return (
    <div className={classes.dropDownButtonWrapper}>
      <LinkButton
        className={`${classes.dropDownButton} ${buttonOpen && classes.upArrow}`}
        disabled={dropDownItems.length < 2}
        onClick={() => {
          setButtonOpen((buttonOpen) => !buttonOpen);
        }}
      >
        <DownArrow />
      </LinkButton>

      {buttonOpen && (
        <div className={classes.panel} ref={panelRef}>
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
