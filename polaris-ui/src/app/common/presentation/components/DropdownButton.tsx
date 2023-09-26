import { useState } from "react";
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
  const [buttonOpen, setButtonOpen] = useState(false);

  const handleBtnClick = (id: string) => {
    setButtonOpen(!buttonOpen);
    callBackFn(id);
  };
  return (
    <div className={classes.dropDownButtonWrapper}>
      <LinkButton
        className={`${classes.dropDownButton} ${buttonOpen && classes.upArrow}`}
        disabled={dropDownItems.length < 2}
        onClick={() => {
          setButtonOpen(!buttonOpen);
        }}
      >
        <DownArrow />
      </LinkButton>

      {buttonOpen && (
        <div className={classes.panel}>
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
