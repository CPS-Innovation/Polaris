import { useState, useEffect, useMemo } from "react";
import {
  Button,
  LinkButton,
  Modal,
} from "../../../../../common/presentation/components/index";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import {
  getLocallySavedRedactionHighlights,
  handleRemoveLocallySavedRedactions,
} from "../../../hooks/utils/redactionUtils";
import { NewPdfHighlight } from "../../../domain/NewPdfHighlight";
import classes from "./UnsavedRedactionModal.module.scss";

type Props = {
  documentId: string;
  caseId: number;
  handleAddRedaction: (newRedaction: NewPdfHighlight[]) => void;
};

export const UnsavedRedactionModal: React.FC<Props> = ({
  documentId,
  caseId,
  handleAddRedaction,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [showModal, setShowModal] = useState(false);
  const locallySavedRedactionHighlights = useMemo(() => {
    return getLocallySavedRedactionHighlights(documentId, caseId);
  }, [documentId, caseId]);
  useEffect(() => {
    if (locallySavedRedactionHighlights.length) {
      setShowModal(true);
    }
  }, []);

  const handleApplyRedaction = () => {
    handleAddRedaction(locallySavedRedactionHighlights);
    trackEvent("Add Unsaved Redactions", {
      documentId: documentId,
      redactionsCount: locallySavedRedactionHighlights.length,
    });
    setShowModal(false);
  };

  const handleIgnoreRedaction = () => {
    trackEvent("Ignore Unsaved Redactions", {
      documentId: documentId,
      redactionsCount: locallySavedRedactionHighlights.length,
    });
    handleRemoveLocallySavedRedactions(documentId, caseId);
    setShowModal(false);
  };

  const description = useMemo(() => {
    return locallySavedRedactionHighlights.length === 1
      ? "You have 1 unsaved redaction on this document, would you like to apply it?"
      : `You have ${locallySavedRedactionHighlights.length} unsaved redactions on this document, would you like to apply it?`;
  }, [locallySavedRedactionHighlights]);

  return (
    <>
      {showModal && (
        <Modal
          isVisible={true}
          className={classes.unsavedRedactionModal}
          ariaLabel="add unsaved redactions modal"
          ariaDescription={description}
          defaultLastFocus={
            document.querySelector("#active-tab-panel") as HTMLElement
          }
        >
          <div className={classes.modalHeader}>
            <h2>
              {locallySavedRedactionHighlights.length === 1
                ? "Apply Unsaved Redaction"
                : "Apply Unsaved Redactions"}
            </h2>
          </div>
          <div className={classes.contentWrapper}>
            <div>
              <span data-testid="unsaved-redactions-description">
                {description}
              </span>
            </div>

            <div className={classes.actionWrapper}>
              <Button
                className={classes.applyBtn}
                onClick={handleApplyRedaction}
                data-testid="btn-apply-redaction"
              >
                Apply
              </Button>
              <LinkButton
                className={classes.ignoreBtn}
                dataTestId="btn-ignore-redaction"
                onClick={handleIgnoreRedaction}
              >
                Ignore
              </LinkButton>
            </div>
          </div>
        </Modal>
      )}
    </>
  );
};
