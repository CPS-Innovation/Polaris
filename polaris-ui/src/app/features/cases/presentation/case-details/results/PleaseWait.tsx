import { Spinner } from "../../../../../common/presentation/components/Spinner";
import classes from "./PleaseWait.module.scss";
type PleaseWaitProps = {
  percentageCompleted: number;
};

export const PleaseWait: React.FC<PleaseWaitProps> = ({
  percentageCompleted,
}) => {
  return (
    <div data-testid="div-please-wait">
      <div className={classes.content}>
        <h1 className="govuk-heading-l ">Loading search results</h1>
        <div> {`Loading... ${percentageCompleted}%`}</div>
        <Spinner
          diameterPx={80}
          ariaLabel="Loading search results, please wait"
        />
      </div>
    </div>
  );
};
