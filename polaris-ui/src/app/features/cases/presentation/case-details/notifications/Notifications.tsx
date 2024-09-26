import { useCallback, useEffect, useRef, useState } from "react";
import {
  NotificationEvent,
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

type Props = {
  state: NotificationState;
  handleNotificationRead: (notificationId: number, documentId: string) => void;
  handleClearAllNotifications: () => void;
  handleClearNotification: (notificationId: number) => void;
};

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

export const Notifications: React.FC<Props> = ({
  state: { events, lastUpdatedDateTime, liveNotificationCount },
  handleNotificationRead,
  handleClearAllNotifications,
  handleClearNotification,
}) => {
  const dropDownBtnRef = useRef<HTMLButtonElement | null>(null);
  const panelRef = useRef<HTMLDivElement | null>(null);
  const [isOpen, setIsOpen] = useState(false);

  const [listRef, handleScroll] =
    useScrollPositionRetention<HTMLUListElement>(isOpen);

  useGlobalDropdownClose(dropDownBtnRef, panelRef, setIsOpen);

  const handleRead = useCallback(
    ({ id, documentId }: NotificationEvent) => {
      handleNotificationRead(id, documentId);
      setIsOpen(false);
    },
    [handleNotificationRead]
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
            liveNotificationCount ? "" : classes.alertEmpty
          }`}
        >
          <span className={classes.count}>{liveNotificationCount}</span>
        </span>
        <span className={classes.label}>Notifications</span>
      </button>
      {isOpen && (
        <div ref={panelRef} className={classes.panel} id="notifications-panel">
          <div
            className={`${classes.header} ${
              liveNotificationCount ? classes.headerPopulated : ""
            }`}
          >
            Last synced with CMS:{" "}
            {lastUpdatedDateTime ? time(lastUpdatedDateTime) : "please wait..."}
          </div>
          {!!liveNotificationCount && (
            <div className={classes.body}>
              <ul ref={listRef} onScroll={handleScroll}>
                {events.map((evt) => (
                  <li
                    key={evt.id}
                    className={
                      evt.status === "Live" ? classes.live : classes.read
                    }
                  >
                    <div>
                      <div>
                        <Tag className={`govuk-tag--orange ${classes.tag}`}>
                          {evt.notificationType}
                        </Tag>
                      </div>
                      <div>
                        <LinkButton
                          className={classes.docLink}
                          onClick={() => handleRead(evt)}
                        >
                          {evt.presentationTitle}
                        </LinkButton>
                      </div>
                      <div className={`govuk-body-s ${classes.narrative}`}>
                        {evt.narrative}
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
