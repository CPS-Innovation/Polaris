import { Button } from "../../../common/presentation/components/Button";
import classes from "./ConfirmationModalContent.module.scss";

type ConfirmationModalContentProps = {
  message: string;
  handleClose: () => void;
};
export const ConfirmationModalContent: React.FC<
  ConfirmationModalContentProps
> = ({ message, handleClose }) => {
  return (
    <div className={classes.confirmationModalContent}>
      <p className={classes.confirmationMessage}>{message}</p>
      <div className={classes.confirmationBtnWrapper}>
        <Button
          className={classes.confirmationOkBtn}
          onClick={handleClose}
          data-testid="btn-confirmation-modal-ok"
        >
          Ok
        </Button>
      </div>
    </div>
  );
};
