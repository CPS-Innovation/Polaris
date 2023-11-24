import { ReactComponent as EditIcon } from "../svgs/edit.svg";
import { LinkButton } from "../components/LinkButton";
import classes from "./EditButton.module.scss";
export type EditButtonProps = {
  id: string;
  value?: string;
  callBackFn: (id: string) => void;
  dataTestId?: string;
  ariaLabel?: string;
};

export const EditButton: React.FC<EditButtonProps> = ({
  id,
  value,
  callBackFn,
  ariaLabel = "edit button",
  dataTestId = "edit-btn",
}) => {
  const handleBtnClick = () => {
    callBackFn(id);
  };

  return (
    <div className={classes.editBtnWrapper}>
      <span>{value}</span>
      <LinkButton
        dataTestId={dataTestId}
        ariaLabel={ariaLabel}
        className={classes.editButton}
        onClick={handleBtnClick}
        type="button"
      >
        <EditIcon />
      </LinkButton>
    </div>
  );
};
