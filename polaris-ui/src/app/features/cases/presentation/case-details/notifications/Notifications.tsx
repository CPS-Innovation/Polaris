import { useMemo, useRef, useState } from "react";
import { NotificationState } from "../../../domain/NotificationState";
import { LinkButton } from "../../../../../common/presentation/components";
import { useGlobalDropdownClose } from "../../../../../common/hooks/useGlobalDropdownClose";
import { filterNotificationsButtonEvents } from "../../../hooks/use-case-details-state/map-notification-state";
import classes from "./notifications.module.scss";
import { Time } from "./Time";
import { Notification } from "./Notification";
import { useScrollPositionRetention } from "./useScrollPositionRetention";
import { useCaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

export const Notifications: React.FC<{
  state: NotificationState;
  handleOpenPdf: ReturnType<typeof useCaseDetailsState>["handleOpenPdf"];
  handleClearAllNotifications: () => void;
  handleClearNotification: (notificationId: number) => void;
}> = ({
  state: { events, lastUpdatedDateTime },
  handleOpenPdf,
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
                    handleOpenPdf={handleOpenPdf}
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
