import { Spinner } from "../../../../../common/presentation/components/Spinner";

import classes from "./Wait.module.scss";

export const Wait: React.FC = () => {
  return (
    <span className={classes.content}>
      <Spinner diameterPx={50} />
    </span>
  );
};
