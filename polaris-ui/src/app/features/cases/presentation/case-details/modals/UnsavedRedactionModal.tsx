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

import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import classes from "./UnsavedRedactionModal.module.scss";

type Props = {
  documentId: string;
  caseId: number;
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
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
    handleAddRedaction(documentId, locallySavedRedactionHighlights);
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
      ? {
          para1:
            "You have 1 unsaved redaction on this document, select OK to apply it.",
          para2:
            "If you do not want to apply the unsaved redaction, select Ignore. Please note: It will not be possible to recover the unsaved redaction if you select this option.",
        }
      : {
          para1: `You have ${locallySavedRedactionHighlights.length} unsaved redactions on this document, select OK to apply them.`,
          para2:
            "If you do not want to apply the unsaved redactions, select Ignore. Please note: It will not be possible to recover the unsaved redactions if you select this option.",
        };
  }, [locallySavedRedactionHighlights]);

  return (
    <>
      {showModal && (
        <Modal
          isVisible={true}
          className={classes.unsavedRedactionModal}
          ariaLabel="add unsaved redactions modal"
          ariaDescription={`${description.para1}${description.para2}`}
          defaultLastFocusId="#active-tab-panel"
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
                <p>{description.para1}</p>
                <p>{description.para2}</p>
              </span>
            </div>

            <div className={classes.actionWrapper}>
              <Button
                className={classes.okBtn}
                onClick={handleApplyRedaction}
                data-testid="btn-apply-redaction"
              >
                OK
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
