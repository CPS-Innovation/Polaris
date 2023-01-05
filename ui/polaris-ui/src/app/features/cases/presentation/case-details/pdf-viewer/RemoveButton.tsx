import classes from "./RemoveButton.module.scss";

type Props = {
  onClick: () => void;
};

export const RemoveButton: React.FC<Props> = ({ onClick }) => (
  <div className="Tip">
    <div className={classes.button} onClick={onClick}>
      Remove redaction
    </div>
  </div>
);
