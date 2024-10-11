import { useState, useEffect } from "react";
import {
  SaveBanner,
  Modal,
} from "../../../../../common/presentation/components/index";
import classes from "./SaveRotationModal.module.scss";

type Props = {
  saveStatus: "saving" | "saved";
};
export const SaveRotationModal: React.FC<Props> = ({ saveStatus }) => {
  const [showModal, setShowModal] = useState(false);

  useEffect(() => {
    if (saveStatus === "saving") {
      setShowModal(true);
    }
  }, [saveStatus]);

  const handleCloseModal = () => {
    setShowModal(false);
  };
  return (
    <Modal
      isVisible={showModal}
      className={classes.savingModal}
      handleClose={saveStatus === "saved" ? handleCloseModal : undefined}
      type="data"
      ariaLabel="Saving document alert modal"
      ariaDescription={
        saveStatus === "saving"
          ? "Saving updated document to CMS..."
          : "Document updated successfully saved to CMS"
      }
    >
      <SaveBanner
        status={saveStatus}
        savingText="Saving updated document to CMS..."
        savedText="Document updated successfully saved to CMS"
      />
    </Modal>
  );
};
