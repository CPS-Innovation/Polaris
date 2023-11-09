import React from "react";
import classes from "./RedactionWarning.module.scss";
import { PresentationFlags } from "../../../domain/gateway/PipelineDocument";
type Props = {
  documentWriteStatus: PresentationFlags["write"];
};

export const RedactionWarning: React.FC<Props> = ({ documentWriteStatus }) => {
  const getWarningText = (documentWriteStatus: PresentationFlags["write"]) => {
    switch (documentWriteStatus) {
      case "IsRedactionServiceOffline":
        return "Redaction is currently unavailable and undergoing maintenance.";
      case "OnlyAvailableInCms":
        return "This document can only be redacted in CMS.";
      case "DocTypeNotAllowed":
        return "Redaction is not supported for this document type.";
      case "OriginalFileTypeNotAllowed":
        return "Redaction is not supported for this file type.";
      case "IsNotOcrProcessed":
        return "Awaiting OCR processing in CMS. Please try again later for redaction.";
    }
  };
  return (
    <div
      className={classes.redactionWarning}
      data-testid="redaction-warning"
      aria-live="polite"
    >
      {getWarningText(documentWriteStatus)}
    </div>
  );
};
