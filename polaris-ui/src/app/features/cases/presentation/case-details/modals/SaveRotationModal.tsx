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
    <div>
      <div aria-live="polite" className={classes.visuallyHidden}>
        {saveStatus === "saving" && (
          <span>Saving updated document to CMS.</span>
        )}

        {saveStatus === "saved" && (
          <span>Document updated successfully saved to CMS</span>
        )}
      </div>
      {saveStatus === "saving" && (
        <Modal
          isVisible={true}
          className={classes.savingModal}
          handleClose={undefined}
          type="data"
          ariaLabel="Saving document alert modal"
          ariaDescription="Saving updated document to CMS"
        >
          <SaveBanner
            status={saveStatus}
            savingText="Saving updated document to CMS..."
            savedText="Document updated successfully saved to CMS"
          />
        </Modal>
      )}

      {saveStatus === "saved" && (
        <Modal
          isVisible={true}
          className={classes.savingModal}
          handleClose={handleCloseSaveRotationModal}
          type="data"
          ariaLabel="Saving document alert modal"
          ariaDescription="Document updated successfully saved to CMS"
        >
          <SaveBanner
            status={saveStatus}
            savingText="Saving updated document to CMS..."
            savedText="Document updated successfully saved to CMS"
          />
        </Modal>
      )}
    </div>
  );
};
