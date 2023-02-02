import React from "react";
import classes from "./RedactButton.module.scss";

type Props = {
  onConfirm: () => void;
};

export const RedactButton: React.FC<Props> = ({ onConfirm }) => {
  return (
    <div
      className={classes.button}
      onClick={onConfirm}
      data-testid="btn-redact"
    >
      Redact
    </div>
  );
};
