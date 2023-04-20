import React from "react";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightTrackEvent";
import classes from "./RedactButton.module.scss";

type Props = {
  onConfirm: () => void;
};

export const RedactButton: React.FC<Props> = ({ onConfirm }) => {
  const trackEvent = useAppInsightsTrackEvent();

  const handleBtnClick = () => {
    trackEvent("Redact content");
    onConfirm();
  };
  return (
    <button
      className={classes.button}
      onClick={handleBtnClick}
      data-testid="btn-redact"
    >
      Redact
    </button>
  );
};
