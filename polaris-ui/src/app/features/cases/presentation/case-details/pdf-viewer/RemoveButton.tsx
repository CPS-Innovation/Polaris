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
    <div id="remove-redact-modal" data-testid="remove-redact-modal">
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
