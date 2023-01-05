import { Spinner } from "../../../../../common/presentation/components/Spinner";
import classes from "./PleaseWait.module.scss";
type Props = {
  handleClose: () => void;
};

export const PleaseWait: React.FC<Props> = ({ handleClose }) => {
  return (
    <div data-testid="div-please-wait">
      <header className="govuk-header" role="banner" data-module="govuk-header">
        <div className={`govuk-header__container  ${classes.header}`}>
          <button
            data-testid="btn-modal-close"
            type="button"
            className={classes.close}
            aria-label="Close"
            onClick={handleClose}
          ></button>
        </div>
      </header>

      <div className={classes.content}>
        <h1 className="govuk-heading-l ">Loading search results</h1>
        <Spinner diameterPx={80} />
      </div>
    </div>
  );
};
