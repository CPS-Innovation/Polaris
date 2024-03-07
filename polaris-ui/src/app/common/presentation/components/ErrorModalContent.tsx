import { useEffect } from "react";
import { Button } from "../../../common/presentation/components/Button";
import { ErrorModalTypes } from "../../../features/cases/domain/ErrorModalTypes";
import { useAppInsightsTrackEvent } from "../../../common/hooks/useAppInsightsTracks";
import classes from "./ErrorModalContent.module.scss";

type ErrorModalContentProps = {
  title: string;
  message: string;
  type: ErrorModalTypes;
  contextData?: {
    documentId?: string;
  };
  handleClose: () => void;
};
export const ErrorModalContent: React.FC<ErrorModalContentProps> = ({
  type,
  title,
  message,
  contextData,
  handleClose,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  useEffect(() => {
    switch (type) {
      case "saveredactionlog":
        trackEvent("Save Redaction Log Error", {
          documentId: contextData?.documentId,
        });
        break;
    }
  }, []);
  const messageParagraphs = message
    .split("<p>")
    .map((item) => item.replace("</p>", ""));
  return (
    <div className={classes.errorModalContent}>
      <h1 className="govuk-heading-l">{title}</h1>
      <div className={classes.errorMessage}>
        {messageParagraphs.map((message, index) => (
          <p key={index}>{message}</p>
        ))}
      </div>
      <div className={classes.errorBtnWrapper}>
        <Button
          className={classes.errorOkBtn}
          onClick={handleClose}
          data-testid="btn-error-modal-ok"
        >
          Ok
        </Button>
      </div>
    </div>
  );
};
