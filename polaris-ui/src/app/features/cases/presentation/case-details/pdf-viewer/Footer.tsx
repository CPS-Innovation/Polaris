import { Button } from "../../../../../common/presentation/components";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import classes from "./Footer.module.scss";

type Props = {
  contextData: {
    documentType: string;
    documentId: string;
    saveStatus: SaveStatus;
  };
  tabIndex: number;
  totalRedactionsCount: number;
  isOkToSave: boolean;
  handleRemoveAllRedactions: () => void;
  handleSavedRedactions: () => void;
};

export const Footer: React.FC<Props> = ({
  contextData,
  tabIndex,
  totalRedactionsCount,
  isOkToSave,
  handleRemoveAllRedactions,
  handleSavedRedactions,
}) => {
  const { documentType, documentId, saveStatus } = contextData;
  const trackEvent = useAppInsightsTrackEvent();
  const handleRemoveAllRedactionsClick = () => {
    trackEvent("Remove All Redactions", {
      documentType: documentType,
      documentId: documentId,
      redactionsCount: totalRedactionsCount,
    });
    handleRemoveAllRedactions();
  };
  const handleSaveAllRedactionsClick = () => {
    handleSavedRedactions();
  };

  return (
    <div className={classes.footer}>
      <LinkButton
        id={`btn-link-removeAll-${tabIndex}`}
        onClick={handleRemoveAllRedactionsClick}
        dataTestId={`btn-link-removeAll-${tabIndex}`}
        disabled={saveStatus.status === "saving"}
        className={classes.removeButton}
      >
        Remove all redactions
      </LinkButton>

      <div
        className={classes.summary}
        data-testid={`redaction-count-text-${tabIndex}`}
      >
        {totalRedactionsCount === 1 ? (
          <>There is 1 redaction</>
        ) : (
          <>There are {totalRedactionsCount} redactions</>
        )}
      </div>

      <Button
        className={classes.saveButton}
        onClick={handleSaveAllRedactionsClick}
        data-testid={`btn-save-redaction-${tabIndex}`}
        disabled={!isOkToSave || saveStatus.status === "saving"}
      >
        Save all redactions
      </Button>
    </div>
  );
};
