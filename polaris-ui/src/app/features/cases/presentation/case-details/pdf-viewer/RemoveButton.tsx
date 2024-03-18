import classes from "./RemoveButton.module.scss";
type Props = {
  onClick: () => void;
};

export const RemoveButton: React.FC<Props> = ({ onClick }) => {
  return (
    <div className="Tip" id="remove-redaction">
      <button
        id="remove-btn"
        data-testid="remove-btn"
        className={classes.button}
        onClick={onClick}
      >
        Remove redaction
      </button>
    </div>
  );
};
