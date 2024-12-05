import { Spinner } from "../../../../../common/presentation/components/Spinner";
import classes from "./PleaseWait.module.scss";
type PleaseWaitProps = {
  percentageCompleted: number;
  showLoadingPercentage: boolean;
};

export const PleaseWait: React.FC<PleaseWaitProps> = ({
  percentageCompleted,
  showLoadingPercentage,
}) => {
  return (
    <div data-testid="div-please-wait">
      <div className={classes.content}>
        <h1 className="govuk-heading-l ">Loading search results</h1>

        <Spinner
          diameterPx={80}
          ariaLabel="Loading search results, please wait"
        />
        {showLoadingPercentage && (
          <div
            className={classes.loadingPercentage}
            data-testid="loading-percentage"
          >
            {" "}
            {`Loading... ${percentageCompleted}%`}
          </div>
        )}
      </div>
    </div>
  );
};
