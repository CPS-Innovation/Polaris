import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightTrackEvent";
import classes from "./RemoveButton.module.scss";

type Props = {
  onClick: () => void;
};

export const RemoveButton: React.FC<Props> = ({ onClick }) => {
  const trackEvent = useAppInsightsTrackEvent();
  const handleOnClick = () => {
    trackEvent("Remove Redact content");
    onClick();
  };
  return (
    <div className="Tip">
      <button className={classes.button} onClick={handleOnClick}>
        Remove redaction
      </button>
    </div>
  );
};
