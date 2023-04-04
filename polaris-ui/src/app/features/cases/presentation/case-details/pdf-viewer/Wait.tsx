import { Spinner } from "../../../../../common/presentation/components/Spinner";

import classes from "./Wait.module.scss";

export const Wait: React.FC<{ dataTestId?: string }> = ({ dataTestId }) => {
  return (
    <div className={classes.content} data-testid={dataTestId}>
      <Spinner diameterPx={50} />
    </div>
  );
};
