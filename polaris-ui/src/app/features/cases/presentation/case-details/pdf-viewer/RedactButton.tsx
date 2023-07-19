import React from "react";
import classes from "./RedactButton.module.scss";

type Props = {
  onConfirm: () => void;
};

export const RedactButton: React.FC<Props> = ({ onConfirm }) => {
  return (
    <button
      className={classes.button}
      onClick={onConfirm}
      data-testid="btn-redact"
      id="btn-redact"
    >
      Redact
    </button>
  );
};
