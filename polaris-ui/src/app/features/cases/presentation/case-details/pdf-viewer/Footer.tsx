import { useMemo } from "react";
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
  redactionHighlightsCount: number;
  suggestedRedactionsCount: number;
  searchPIIHighlightsCount: number;
  isOkToSave: boolean;
  handleRemoveAllRedactions: () => void;
  handleSavedRedactions: () => void;
};

export const Footer: React.FC<Props> = ({
  contextData,
  tabIndex,
  redactionHighlightsCount,
  suggestedRedactionsCount,
  searchPIIHighlightsCount,
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
      redactionsCount: redactionHighlightsCount,
    });
    handleRemoveAllRedactions();
  };
  const handleSaveAllRedactionsClick = () => {
    trackEvent("Save All Redactions", {
      documentType: documentType,
      documentId: documentId,
      redactionsCount: redactionHighlightsCount,
    });
    handleSavedRedactions();
  };

  const getTotalRedactions = () => {
    return suggestedRedactionsCount + redactionHighlightsCount;
  };
  return (
    <div className={classes.footer}>
      <LinkButton
        id={`btn-link-removeAll-${tabIndex}`}
        onClick={handleRemoveAllRedactionsClick}
        dataTestId={`btn-link-removeAll-${tabIndex}`}
        disabled={saveStatus === "saving"}
        className={classes.removeButton}
      >
        Remove all redactions
      </LinkButton>

      <div
        className={classes.summary}
        data-testid={`redaction-count-text-${tabIndex}`}
      >
        {getTotalRedactions() === 1 ? (
          <>There is 1 redaction</>
        ) : (
          <>There are {getTotalRedactions()} redactions</>
        )}
      </div>

      <Button
        className={classes.saveButton}
        onClick={handleSaveAllRedactionsClick}
        data-testid={`btn-save-redaction-${tabIndex}`}
        disabled={!isOkToSave || saveStatus === "saving"}
      >
        Save all redactions
      </Button>
    </div>
  );
};
