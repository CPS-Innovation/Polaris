import { useState } from "react";
import { NotificationState } from "../../../domain/NotificationState";
import classes from "./notifications.module.scss";
type Props = {
  state: NotificationState;
};

export const Notifications: React.FC<Props> = ({ state }) => {
  const [isOpen, setIsOpen] = useState<boolean>();

  return (
    <button className={classes.root}>
      <span
        className={`${classes.alert} ${
          state.events.length === 0 && classes.alertEmpty
        }`}
      >
        <span className={classes.count}>{state.events.length}</span>
      </span>
      <span className={classes.label}>Notifications</span>
    </button>
  );
};
