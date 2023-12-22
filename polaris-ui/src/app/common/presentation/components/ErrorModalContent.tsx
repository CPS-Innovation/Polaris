import { Button } from "../../../common/presentation/components/Button";
import classes from "./ErrorModalContent.module.scss";

type ErrorModalContentProps = {
  title: string;
  message: string;
  handleClose: () => void;
};
export const ErrorModalContent: React.FC<ErrorModalContentProps> = ({
  title,
  message,
  handleClose,
}) => {
  const messageParagraphs = message
    .split("<p>")
    .map((item) => item.replace("</p>", ""));
  return (
    <div className={classes.errorModalContent}>
      <h1 className="govuk-heading-l">{title}</h1>
      <div className={classes.errorMessage}>
        {messageParagraphs.map((message) => (
          <p>{message}</p>
        ))}
      </div>
      <div className={classes.errorBtnWrapper}>
        <Button
          className={classes.errorOkBtn}
          onClick={handleClose}
          data-testid="btn-error-modal-ok"
        >
          Ok
        </Button>
      </div>
    </div>
  );
};
