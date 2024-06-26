import { Spinner } from "../../../../../common/presentation/components/Spinner";

import classes from "./Wait.module.scss";
type WaitProps = {
  dataTestId?: string;
  ariaLabel: string;
};
export const Wait: React.FC<WaitProps> = ({
  dataTestId = "div-loading-spinner",
  ariaLabel,
}) => {
  return (
    <div className={classes.content} data-testid={dataTestId}>
      <div className={classes.spinnerWrapper}>
        <Spinner diameterPx={50} ariaLabel={ariaLabel} />
      </div>
    </div>
  );
};
