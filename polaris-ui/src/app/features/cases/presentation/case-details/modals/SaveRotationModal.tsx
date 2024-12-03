import {
  SaveBanner,
  Modal,
} from "../../../../../common/presentation/components/index";
import classes from "./SaveRotationModal.module.scss";

type Props = {
  saveStatus: "saving" | "saved";
  handleCloseSaveRotationModal: () => void;
};
export const SaveRotationModal: React.FC<Props> = ({
  saveStatus,
  handleCloseSaveRotationModal,
}) => {
  return (
    <Modal
      isVisible={true}
      className={classes.savingModal}
      handleClose={
        saveStatus === "saved" ? handleCloseSaveRotationModal : undefined
      }
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
