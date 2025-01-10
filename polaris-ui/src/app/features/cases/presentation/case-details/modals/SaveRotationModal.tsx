import {
  Modal,
  Spinner,
} from "../../../../../common/presentation/components/index";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import { ReactComponent as WhiteTickIcon } from "../../../../../common/presentation/svgs/whiteTick.svg";
import classes from "./SaveRotationModal.module.scss";

type Props = {
  saveStatus: SaveStatus;
  handleCloseSaveRotationModal: () => void;
};
export const SaveRotationModal: React.FC<Props> = ({
  saveStatus,
  handleCloseSaveRotationModal,
}) => {
  return (
    <div>
      <div aria-live="polite" className={classes.visuallyHidden}>
        {saveStatus.type === "rotation" && saveStatus.status === "saving" && (
          <span>Saving updated document to CMS</span>
        )}
      </div>
      {saveStatus.type === "rotation" && saveStatus.status === "saving" && (
        <Modal
          isVisible={true}
          className={classes.savingModal}
          handleClose={undefined}
          type="data"
          ariaLabel="Document saving alert modal"
          ariaDescription="Saving updated document to CMS"
        >
          <div
            className={classes.savingBanner}
            data-testid="rl-saving-redactions"
          >
            <div className={classes.spinnerWrapper}>
              <Spinner diameterPx={15} ariaLabel={"spinner-animation"} />
            </div>
            <h2 className={classes.bannerText}>
              Saving updated document to CMS...
            </h2>
          </div>
        </Modal>
      )}

      {saveStatus.type === "rotation" && saveStatus.status === "saved" && (
        <Modal
          isVisible={true}
          className={classes.savingModal}
          handleClose={handleCloseSaveRotationModal}
          type="data"
          ariaLabel="Document saved alert modal"
          ariaDescription="Document updated successfully saved to CMS"
        >
          <div
            className={classes.savedBanner}
            data-testid="rl-saved-redactions"
          >
            <WhiteTickIcon className={classes.whiteTickIcon} />
            <h2 className={classes.bannerText}>
              Document updated successfully saved to CMS
            </h2>
          </div>
        </Modal>
      )}
    </div>
  );
};
