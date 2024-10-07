import { ReactComponent as TimeIcon } from "../../../../../common/presentation/svgs/time.svg";
import classes from "./notifications.module.scss";
import { formatTime } from "../../../../../common/utils/dates";

export const Time: React.FC<{ dateTime: string }> = ({ dateTime }) => (
  <span className={classes.time}>
    <TimeIcon className={classes.timeIcon} />
    {formatTime(dateTime)}
  </span>
);
