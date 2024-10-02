import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  NotificationEvent,
  NotificationReasonMap,
  NotificationState,
} from "../../../domain/NotificationState";
import { formatTime } from "../../../../../common/utils/dates";
import classes from "./notifications.module.scss";
import {
  Button,
  LinkButton,
  Tag,
} from "../../../../../common/presentation/components";
import { useGlobalDropdownClose } from "../../../../../common/hooks/useGlobalDropdownClose";
import { ReactComponent as TimeIcon } from "../../../../../common/presentation/svgs/time.svg";
import { filterNotificationsButtonEvents } from "../../../hooks/use-case-details-state/map-notification-state";

const time = (dateTime: string) => (
  <span className={classes.time}>
    <TimeIcon className={classes.timeIcon} />
    {formatTime(dateTime)}
  </span>
);

const useScrollPositionRetention = <T extends HTMLElement>(
  isElOpen: boolean
) => {
  const elRef = useRef<T | null>(null);
  const [scrollPosition, setScrollPosition] = useState<number>();

  const handleScroll: React.UIEventHandler<T> = (event) => {
    setScrollPosition((event?.target as HTMLElement)?.scrollTop);
  };

  useEffect(() => {
    if (isElOpen && elRef.current && scrollPosition !== undefined) {
      elRef.current.scrollTo({ top: scrollPosition });
    }
  }, [isElOpen, scrollPosition]);

  return [elRef, handleScroll] as const;
};

const Notification: React.FC<{
  evt: NotificationEvent;
  handleReadNotification: ({ id, documentId }: NotificationEvent) => void;
  handleClearNotification: (id: number) => void;
}> = ({ evt, handleReadNotification, handleClearNotification }) => {
  return (
    <li
      key={evt.id}
      className={evt.status === "Live" ? classes.live : classes.read}
    >
      <div>
        <div>
          <Tag
            gdsTagColour={NotificationReasonMap[evt.reason]}
            className={classes.tag}
          >
            {evt.reason}
          </Tag>
        </div>
        <div>
          <LinkButton
            className={classes.docLink}
            onClick={() => handleReadNotification(evt)}
          >
            {evt.presentationTitle}
          </LinkButton>
        </div>
        <div className={`govuk-body-s ${classes.narrative}`}>
          <span>{evt.narrative}</span>
          {evt.reasonToIgnore ? (
            <span className={classes.reasonToIgnore}>{evt.reasonToIgnore}</span>
          ) : undefined}
        </div>
        <span className={`${classes.dateTime} govuk-body-s`}>
          {time(evt.dateTime)}
        </span>
      </div>
      <div>
        <Button
          data-prevent-global-close
          onClick={() => handleClearNotification(evt.id)}
          className={`${classes.clear} govuk-button--secondary`}
        >
          Clear
        </Button>
      </div>
    </li>
  );
};

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
            {lastUpdatedDateTime ? time(lastUpdatedDateTime) : "please wait..."}
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
