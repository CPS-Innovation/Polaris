import React from "react";
import classes from "./RedactButton.module.scss";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";

type Props = {
  onConfirm: () => void;
};

export const RedactButton: React.FC<Props> = ({ onConfirm }) => {
  useFocusTrap("#redact-modal");
  useLastFocus();
  return (
    <div id="redact-modal" role="alertdialog" aria-modal="true">
      <button
        className={classes.button}
        onClick={onConfirm}
        data-testid="btn-redact"
        id="btn-redact"
      >
        Redact
      </button>
    </div>
  );
};
