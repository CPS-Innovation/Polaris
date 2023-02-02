import { Spinner } from "../../../../../common/presentation/components/Spinner";
import classes from "./PleaseWait.module.scss";

export const PleaseWait: React.FC = () => {
  return (
    <div data-testid="div-please-wait">
      <div className={classes.content}>
        <h1 className="govuk-heading-l ">Loading search results</h1>
        <Spinner diameterPx={80} />
      </div>
    </div>
  );
};
