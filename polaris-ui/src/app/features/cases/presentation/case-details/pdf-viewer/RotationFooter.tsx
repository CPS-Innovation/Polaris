import { Button } from "../../../../../common/presentation/components";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import classes from "./Footer.module.scss";

type RotationFooterProps = {
  contextData: {
    documentType: string;
    documentId: string;
    saveStatus: SaveStatus;
  };
  tabIndex: number;
  totalRotationsCount: number;
  isOkToSave: boolean;
  handleRemoveAllRotations: CaseDetailsState["handleRemoveAllRotations"];
  handleSaveRotations: CaseDetailsState["handleSaveRotations"];
};

export const RotationFooter: React.FC<RotationFooterProps> = ({
  contextData,
  tabIndex,
  totalRotationsCount,
  isOkToSave,
  handleRemoveAllRotations,
  handleSaveRotations,
}) => {
  const { documentType, documentId, saveStatus } = contextData;
  const trackEvent = useAppInsightsTrackEvent();
  const handleRemoveAllRotationsClick = () => {
    trackEvent("Remove All Rotations", {
      documentType: documentType,
      documentId: documentId,
      rotatedPagesCount: totalRotationsCount,
    });
    handleRemoveAllRotations(documentId);
  };
  const handleSaveRotationsClick = () => {
    trackEvent("Save Rotation", {
      documentType: documentType,
      documentId: documentId,
      rotatedPagesCount: totalRotationsCount,
    });
    handleSaveRotations(documentId);
  };

  return (
    <div className={classes.footer}>
      <LinkButton
        id={`btn-link-removeAll-rotation-${tabIndex}`}
        onClick={handleRemoveAllRotationsClick}
        dataTestId={`btn-link-removeAll-${tabIndex}`}
        disabled={saveStatus.status === "saving"}
        className={classes.removeButton}
      >
        Remove all rotations
      </LinkButton>

      <div
        className={classes.summary}
        data-testid={`rotation-count-text-${tabIndex}`}
      >
        {totalRotationsCount === 1 ? (
          <>There is 1 rotation</>
        ) : (
          <>There are {totalRotationsCount} rotations</>
        )}
      </div>

      <Button
        className={classes.saveButton}
        onClick={handleSaveRotationsClick}
        data-testid={`btn-save-rotations-${tabIndex}`}
        disabled={!isOkToSave || saveStatus.status === "saving"}
      >
        Save all rotations
      </Button>
    </div>
  );
};
