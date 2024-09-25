import { useState } from "react";
import { NotificationState } from "../../../domain/NotificationState";
import { formatTime } from "../../../../../common/utils/dates";
import classes from "./notifications.module.scss";
import { LinkButton } from "../../../../../common/presentation/components";

type Props = {
  state: NotificationState;
  handleClearAllNotifications: () => void;
};

export const Notifications: React.FC<Props> = ({
  state: { events, lastUpdatedDateTime, liveNotificationCount },
  handleClearAllNotifications,
}) => {
  const [isOpen, setIsOpen] = useState<boolean>();

  return (
    <div className={classes.root}>
      <button className={classes.btn} onClick={() => setIsOpen(!isOpen)}>
        <span
          className={`${classes.alert} ${
            liveNotificationCount ? "" : classes.alertEmpty
          }`}
        >
          <span className={classes.count}>{liveNotificationCount}</span>
        </span>
        <span className={classes.label}>Notifications</span>
      </button>
      {isOpen && (
        <div className={classes.panel}>
          <div className={classes.header}>
            Last checked:{" "}
            {lastUpdatedDateTime
              ? formatTime(lastUpdatedDateTime)
              : "please wait..."}
          </div>
          {!!liveNotificationCount && (
            <>
              <div className={classes.list}></div>
              <div className={classes.footer}>
                <LinkButton onClick={handleClearAllNotifications}>
                  Clear all notifications
                </LinkButton>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
};
