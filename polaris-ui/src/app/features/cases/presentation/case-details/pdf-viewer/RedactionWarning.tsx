import React from "react";
import classes from "./RedactionWarning.module.scss";

export const RedactionWarning: React.FC = () => {
  return (
    <div className={classes.redactionWarning} data-testid="redaction-warning">
      This document can only be redacted in CMS.
    </div>
  );
};
