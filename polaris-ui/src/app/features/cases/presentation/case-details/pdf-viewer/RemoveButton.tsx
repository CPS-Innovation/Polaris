import classes from "./RemoveButton.module.scss";

type Props = {
  onClick: () => void;
};

export const RemoveButton: React.FC<Props> = ({ onClick }) => (
  <div className="Tip">
    <button className={classes.button} onClick={onClick}>
      Remove redaction
    </button>
  </div>
);
