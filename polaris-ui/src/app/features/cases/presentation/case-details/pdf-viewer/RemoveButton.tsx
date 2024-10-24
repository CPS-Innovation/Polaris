import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";
import classes from "./RemoveButton.module.scss";

type Props = {
  onClick: () => void;
};

export const RemoveButton: React.FC<Props> = ({ onClick }) => {
  useFocusTrap("#remove-redact-modal");
  useLastFocus();
  return (
    <div
      id="remove-redact-modal"
      data-testid="remove-redact-modal"
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="remove-redact-modal-label"
      aria-describedby="remove-redact-modal-description"
    >
      <span id="remove-redact-modal-label" className={classes.visuallyHidden}>
        remove redaction modal
      </span>
      <span id="redact-modal-description" className={classes.visuallyHidden}>
        A modal with a remove redaction button to confirm the removal of unsaved
        redaction
      </span>
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
    </div>
  );
};
