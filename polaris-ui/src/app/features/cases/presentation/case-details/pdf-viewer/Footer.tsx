import { Button } from "../../../../../common/presentation/components";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { SavingStatus } from "../../../domain/gateway/SavingStatus";
import classes from "./Footer.module.scss";

type Props = {
  contextData: {
    documentType: string;
    documentId: string;
    savingStatus: SavingStatus;
  };
  tabIndex: number;
  redactionHighlights: IPdfHighlight[];
  isOkToSave: boolean;
  handleRemoveAllRedactions: () => void;
  handleSavedRedactions: () => void;
};

export const Footer: React.FC<Props> = ({
  contextData,
  tabIndex,
  redactionHighlights,
  isOkToSave,
  handleRemoveAllRedactions,
  handleSavedRedactions,
}) => {
  const { documentType, documentId, savingStatus } = contextData;
  const trackEvent = useAppInsightsTrackEvent();
  const handleRemoveAllRedactionsClick = () => {
    trackEvent("Remove All Redactions", {
      documentType: documentType,
      documentId: documentId,
      redactionsCount: redactionHighlights.length,
    });
    handleRemoveAllRedactions();
  };
  const handleSaveAllRedactionsClick = () => {
    trackEvent("Save All Redactions", {
      documentType: documentType,
      documentId: documentId,
      redactionsCount: redactionHighlights.length,
    });
    handleSavedRedactions();
  };
  return (
    <div className={classes.footer}>
      <LinkButton
        id={`btn-link-removeAll-${tabIndex}`}
        onClick={handleRemoveAllRedactionsClick}
        dataTestId={`btn-link-removeAll-${tabIndex}`}
        disabled={savingStatus === "saving"}
        className={classes.removeButton}
      >
        Remove all redactions
      </LinkButton>

      <div className={classes.summary} data-testid={`redaction-count-text`}>
        {redactionHighlights.length === 1 ? (
          <>There is 1 redaction</>
        ) : (
          <>There are {redactionHighlights.length} redactions</>
        )}
      </div>

      <Button
        className={classes.saveButton}
        onClick={handleSaveAllRedactionsClick}
        data-testid={`btn-save-redaction-${tabIndex}`}
        disabled={!isOkToSave || savingStatus === "saving"}
      >
        Save all redactions
      </Button>
    </div>
  );
};
