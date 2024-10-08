import {
  LinkButton,
  Modal,
} from "../../../../../common/presentation/components/index";
import classes from "./PageRotationWarningModal.module.scss";

type Props = {
  hidePageRotationWarningModal: () => void;
};

export const PageRotationWarningModal: React.FC<Props> = ({
  hidePageRotationWarningModal,
}) => {
  const handleCloseRotationWarningModal = () => {
    hidePageRotationWarningModal();
  };

  return (
    <Modal
      isVisible={true}
      handleClose={handleCloseRotationWarningModal}
      className={classes.rotationWarningModal}
      ariaLabel="Confirm redaction suggestions Modal"
      ariaDescription={`You cannot rotate pages as you have unsaved redactions and these
              will be lost. Remove or save your redactions and you will be able to continue.`}
      defaultLastFocusId="#active-tab-panel"
    >
      <div className={classes.modalHeader}>
        <h2 className={classes.headerText}>Save your redactions</h2>
      </div>
      <div className={classes.contentWrapper}>
        <div className={classes.mainText}>
          <span className="govuk-warning-text__icon" aria-hidden="true">
            !
          </span>
          <p className={classes.contentText}>
            You cannot rotate pages as you have unsaved redactions and these
            will be lost.
          </p>
        </div>
        <p className={classes.subText}>
          Remove or save your redactions and you will be able to continue.
        </p>
        <div className={classes.actionWrapper}>
          <LinkButton
            dataTestId="btn-close"
            onClick={handleCloseRotationWarningModal}
          >
            Close
          </LinkButton>
        </div>
      </div>
    </Modal>
  );
};
