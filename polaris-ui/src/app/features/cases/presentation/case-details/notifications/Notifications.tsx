import { useCallback, useMemo, useRef, useState } from "react";
import {
  NotificationEvent,
  NotificationState,
} from "../../../domain/NotificationState";
import { LinkButton } from "../../../../../common/presentation/components";
import { useGlobalDropdownClose } from "../../../../../common/hooks/useGlobalDropdownClose";
import { filterNotificationsButtonEvents } from "../../../hooks/use-case-details-state/map-notification-state";
import classes from "./notifications.module.scss";
import { Time } from "./Time";
import { Notification } from "./Notification";
import { useScrollPositionRetention } from "./useScrollPositionRetention";

export const Notifications: React.FC<{
  state: NotificationState;
  handleReadNotification: (
    notificationId: number,
    documentId: string,
    shouldOpenDoc: boolean
  ) => void;
  handleClearAllNotifications: () => void;
  handleClearNotification: (notificationId: number) => void;
}> = ({
  state: { events, lastUpdatedDateTime },
  handleReadNotification,
  handleClearAllNotifications,
  handleClearNotification,
}) => {
  const dropDownBtnRef = useRef<HTMLButtonElement | null>(null);
  const panelRef = useRef<HTMLDivElement | null>(null);
  const [isOpen, setIsOpen] = useState(false);

  const [listRef, handleScroll] =
    useScrollPositionRetention<HTMLUListElement>(isOpen);

  useGlobalDropdownClose(
    dropDownBtnRef,
    panelRef,
    setIsOpen,
    "#notifications-panel"
  );

  const localHandleNotificationRead = useCallback(
    (evt: NotificationEvent) => {
      handleReadNotification(
        evt.id,
        evt.documentId,
        evt.reason !== "Discarded"
      );
      // special case: if this is a "Discarded" document then there is nothing to open
      //  so lets keep the user with the panel open.
      if (evt.reason !== "Discarded") {
        setIsOpen(false);
      }
    },
    [handleReadNotification]
  );

  const { liveEventCount, eventsToDisplay } = useMemo(
    () => filterNotificationsButtonEvents(events),
    [events]
  );

  return (
    <div className={classes.root}>
      <button
        ref={dropDownBtnRef}
        className={classes.btn}
        onClick={() => setIsOpen(!isOpen)}
      >
        <span
          className={`${classes.alert} ${
            liveEventCount ? "" : classes.alertEmpty
          }`}
        >
          <span className={classes.count}>{liveEventCount}</span>
        </span>
        <span className={classes.label}>Notifications</span>
      </button>
      {isOpen && (
        <div ref={panelRef} className={classes.panel} id="notifications-panel">
          <div
            className={`${classes.header} ${
              liveEventCount ? classes.headerPopulated : ""
            }`}
          >
            Last synced with CMS:{" "}
            {lastUpdatedDateTime ? (
              <Time dateTime={lastUpdatedDateTime} />
            ) : (
              "please wait..."
            )}
          </div>
          {!!eventsToDisplay.length && (
            <div className={classes.body}>
              <ul ref={listRef} onScroll={handleScroll}>
                {events.map((evt) => (
                  <Notification
                    key={evt.id}
                    evt={evt}
                    handleReadNotification={localHandleNotificationRead}
                    handleClearNotification={handleClearNotification}
                  ></Notification>
                ))}
              </ul>
              <div className={classes.footer}>
                <LinkButton onClick={handleClearAllNotifications}>
                  Clear all notifications
                </LinkButton>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
