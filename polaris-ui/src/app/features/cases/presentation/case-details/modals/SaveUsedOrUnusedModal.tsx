import { useState, useEffect } from "react";
import {
  LinkButton,
  Modal,
  Spinner,
} from "../../../../../common/presentation/components/index";
import { ReactComponent as WhiteTickIcon } from "../../../../../common/presentation/svgs/whiteTick.svg";
import { ReactComponent as CloseIcon } from "../../../../../common/presentation/svgs/closeIconBold.svg";
import classes from "./SaveUsedOrUnusedModal.module.scss";

type Props = {
  savingState?: string;
};
export const SaveUsedOrUnusedModal: React.FC<Props> = ({ savingState }) => {
  const [showModal, setShowModal] = useState(false);

  useEffect(() => {
    if (savingState === "saving") setShowModal(true);
  }, [savingState]);

  const handleCloseModal = () => {
    setShowModal(false);
  };

  return (
    <Modal
      isVisible={showModal ? true : false}
      handleClose={savingState === "saved" ? handleCloseModal : undefined}
      type="alert"
      ariaLabel="Saving document state alert modal"
      ariaDescription={"Document updated state successfully saved to CMS"}
      className={classes.modalUsedUnusedModal}
    >
      <>
        {savingState === "saving" && (
          <div
            className={classes.savingBanner}
            data-testid="rl-saving-redactions"
          >
            <div className={classes.spinnerWrapper}>
              <Spinner diameterPx={15} ariaLabel={"spinner-animation"} />
            </div>
            &nbsp; Saving document's state to CMS
          </div>
        )}
        {savingState === "success" && (
          <div
            className={classes.savedBanner}
            data-testid="rl-saved-redactions"
          >
            <WhiteTickIcon className={classes.whiteTickIcon} />
            &nbsp; Document's state successfully saved to CMS {savingState}
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
        )}
      </>
    </Modal>
  );
};
