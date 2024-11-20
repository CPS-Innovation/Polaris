import { useMemo } from "react";
import {
  LinkButton,
  Modal,
} from "../../../../../common/presentation/components/index";
import classes from "./PageRotationDeletionWarningModal.module.scss";

type Props = {
  type?: RotationDeletionWarningModal;
  hidePageRotationDeletionWarningModal: () => void;
};

export type RotationDeletionWarningModal =
  | "HideDeletionWarning"
  | "HideRotationWarning"
  | "ShowRotationWarning"
  | "ShowDeletionWarning";

export const PageRotationDeletionWarningModal: React.FC<Props> = ({
  type,
  hidePageRotationDeletionWarningModal,
}) => {
  const handleCloseRotationDeletionWarningModal = () => {
    hidePageRotationDeletionWarningModal();
  };

  const contentTexts = useMemo(() => {
    switch (type) {
      case "ShowRotationWarning":
        return {
          heading: "Save your redactions",
          mainContent:
            "You cannot rotate pages as you have unsaved redactions and these will be lost.",
          subContent:
            "Remove or save your redactions and you will be able to continue.",
        };
      case "HideRotationWarning":
        return {
          heading: "Save your rotations",
          mainContent:
            "You cannot turn off rotation feature as you have unsaved rotations and these will be lost.",
          subContent:
            "Remove or save your rotations and you will be able to continue.",
        };
      case "ShowDeletionWarning":
        return {
          heading: "Save your rotations",
          mainContent:
            "You cannot delete pages as you have unsaved rotations and these will be lost.",
          subContent:
            "Remove or save your rotations and you will be able to continue.",
        };
      case "HideDeletionWarning":
        return {
          heading: "Save your redactions",
          mainContent:
            "You cannot turn off deletion feature as you have unsaved redactions and these will be lost.",
          subContent:
            "Remove or save your redactions and you will be able to continue.",
        };
      default:
        return {};
    }
  }, [type]);

  return (
    <Modal
      isVisible={true}
      handleClose={handleCloseRotationDeletionWarningModal}
      className={classes.rotationDeletionWarningModal}
      ariaLabel={`${contentTexts.heading} warning modal`}
      ariaDescription={`${contentTexts.heading}${contentTexts.mainContent}${contentTexts.subContent}`}
      defaultLastFocusId="#active-tab-panel"
    >
      <div className={classes.modalHeader}>
        <h2 className={classes.headerText}>{contentTexts.heading}</h2>
      </div>
      <div className={classes.contentWrapper}>
        <div className={classes.mainText}>
          <span className="govuk-warning-text__icon" aria-hidden="true">
            !
          </span>
          <p className={classes.contentText}>{contentTexts.mainContent}</p>
        </div>
        <p className={classes.subText}>{contentTexts.subContent}</p>
        <div className={classes.actionWrapper}>
          <LinkButton
            dataTestId="btn-close"
            onClick={handleCloseRotationDeletionWarningModal}
          >
            Close
          </LinkButton>
        </div>
      </div>
    </Modal>
  );
};
