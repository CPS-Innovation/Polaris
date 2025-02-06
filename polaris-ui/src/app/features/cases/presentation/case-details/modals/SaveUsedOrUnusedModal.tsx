import { useState, useEffect } from "react";
import {
  LinkButton,
  Modal,
  Spinner,
} from "../../../../../common/presentation/components/index";
import { ReactComponent as WhiteTickIcon } from "../../../../../common/presentation/svgs/whiteTick.svg";
import { ReactComponent as CloseIcon } from "../../../../../common/presentation/svgs/closeIconBold.svg";
import classes from "./SaveUsedOrUnusedModal.module.scss";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";

type Props = {
  savingState?: string;
  documentId: string;
};
export const SaveUsedOrUnusedModal: React.FC<Props> = ({
  savingState,
  documentId,
}) => {
  const [showModal, setShowModal] = useState(false);

  const trackEvent = useAppInsightsTrackEvent();

  useEffect(() => {
    if (savingState === "saving") setShowModal(true);
  }, [savingState]);

  const handleCloseModal = () => {
    trackEvent("Ignore Saved Or Unsaved Redactions Modal Window", {
      documentId: documentId,
    });
    setShowModal(false);
  };

  return (
    <>
      <div aria-live="polite" className={classes.visuallyHidden}>
        {savingState === "saving" && (
          <span>Saving updated document to CMS</span>
        )}
      </div>
      {savingState === "saving" && (
        <Modal
          isVisible={showModal}
          handleClose={savingState === "saving" ? handleCloseModal : undefined}
          type="alert"
          ariaLabel="Saving updated alert modal"
          ariaDescription={"Saving updated document to CMS"}
          className={classes.modalUsedUnusedModal}
        >
          <div
            className={classes.savingBanner}
            data-testid="rl-saving-redactions"
          >
            <div className={classes.spinnerWrapper}>
              <Spinner diameterPx={15} ariaLabel={"spinner-animation"} />
            </div>
            &nbsp; Saving updated document to CMS
          </div>
        </Modal>
      )}
      {savingState === "success" && (
        <Modal
          isVisible={showModal}
          handleClose={savingState === "success" ? handleCloseModal : undefined}
          type="alert"
          ariaLabel="Document saved alert modal"
          ariaDescription={"Document successfully saved to CMS"}
          className={classes.modalUsedUnusedModal}
        >
          <div
            className={classes.savedBanner}
            data-testid="rl-saved-redactions"
          >
            <WhiteTickIcon className={classes.whiteTickIcon} />
            &nbsp; Document successfully saved to CMS
            <LinkButton
              dataTestId="btn-close-used-unused-state-panel"
              type="button"
              className={classes.renamePanelCloseBtn}
              ariaLabel="close panel"
              disabled={false}
              onClick={handleCloseModal}
            >
              <CloseIcon height={"2.7rem"} width={"2.7rem"} />
            </LinkButton>
          </div>
        </Modal>
      )}
    </>
  );
};
