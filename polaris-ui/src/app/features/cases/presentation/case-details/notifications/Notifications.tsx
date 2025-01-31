import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { NotificationState } from "../../../domain/NotificationState";
import { LinkButton } from "../../../../../common/presentation/components";
import { useGlobalDropdownClose } from "../../../../../common/hooks/useGlobalDropdownClose";
import { filterNotificationsButtonEvents } from "../../../hooks/use-case-details-state/map-notification-state";
import classes from "./notifications.module.scss";
import { Time } from "./Time";
import { Notification } from "./Notification";
import { useScrollPositionRetention } from "./useScrollPositionRetention";
import { useCaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";

export const Notifications: React.FC<{
  state: NotificationState;
  handleOpenPdf: ReturnType<typeof useCaseDetailsState>["handleOpenPdf"];
  handleClearAllNotifications: () => void;
  handleClearNotification: (notificationId: number) => void;
}> = ({
  state: { events, lastCheckedDateTime },
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

  const trackEvent = useAppInsightsTrackEvent();

  const { liveEventCount, eventsToDisplay } = useMemo(
    () => filterNotificationsButtonEvents(events),
    [events]
  );

  const [lastLiveEventCount, setLastLiveEventCount] = useState(0);
  useEffect(() => {
    if (liveEventCount > lastLiveEventCount) {
      trackEvent("Notifications Arrived", {
        liveEventCount,
        lastLiveEventCount,
      });
    }
    setLastLiveEventCount(liveEventCount);
  }, [liveEventCount, lastLiveEventCount, trackEvent, setLastLiveEventCount]);

  useEffect(() => {
    if (isOpen) {
      trackEvent("Notification Panel Opened");
    }
  }, [isOpen, trackEvent]);

  const localHandleOpenPdf = useCallback(
    ({ documentId, mode }) => {
      trackEvent("Document Opened from Notification", { documentId });
      handleOpenPdf({ documentId, mode });
      if (isOpen) {
        setIsOpen(false);
      }
    },
    [handleOpenPdf, trackEvent, isOpen]
  );

  const localHandleClearNotification = useCallback(
    (notificationId: number, documentId: string) => {
      trackEvent("Notification Cleared", { notificationId, documentId });
      handleClearNotification(notificationId);
      if (isOpen && eventsToDisplay.length === 1) {
        setIsOpen(false);
        dropDownBtnRef.current?.focus();
      }
    },
    [handleClearNotification, trackEvent, eventsToDisplay, isOpen]
  );

  const localHandleClearAllNotifications = useCallback(() => {
    trackEvent("All Notifications Cleared");
    handleClearAllNotifications();
    if (isOpen) {
      setIsOpen(false);
      dropDownBtnRef.current?.focus();
    }
  }, [handleClearAllNotifications, trackEvent, isOpen]);

  return (
    <div className={classes.root}>
      <div
        aria-live="polite"
        className={classes.visuallyHidden}
      >{`you have ${liveEventCount} notifications`}</div>
      <button
        data-testid="notifications_btn"
        ref={dropDownBtnRef}
        aria-expanded={isOpen}
        aria-label={
          liveEventCount > 0
            ? `open ${liveEventCount} notifications`
            : "open notifications"
        }
        className={classes.btn}
        onClick={() => setIsOpen(!isOpen)}
      >
        <span
          className={`${classes.alert} ${
            liveEventCount ? "" : classes.alertEmpty
          }`}
        >
          <span data-testid="notifications_count" className={classes.count}>
            {liveEventCount}
          </span>
        </span>
        <span className={classes.label}>Notifications</span>
      </button>
      {isOpen && (
        <div
          ref={panelRef}
          className={classes.panel}
          id="notifications-panel"
          data-testid="notifications-panel"
        >
          <div
            className={`${classes.header} ${
              liveEventCount ? classes.headerPopulated : ""
            }`}
          >
            Last synced with CMS:{" "}
            {lastCheckedDateTime ? (
              <Time dateTime={lastCheckedDateTime} />
            ) : (
              "please wait..."
            )}
          </div>
          {!!eventsToDisplay.length && (
            <div className={classes.body}>
              <div className={classes.wrapper}>
                <ul
                  className={classes.notificationsList}
                  ref={listRef}
                  onScroll={handleScroll}
                >
                  {eventsToDisplay.map((evt) => (
                    <Notification
                      key={evt.id}
                      evt={evt}
                      handleOpenPdf={localHandleOpenPdf}
                      handleClearNotification={(id) =>
                        localHandleClearNotification(id, evt.documentId)
                      }
                    ></Notification>
                  ))}
                </ul>
              </div>
              <div className={classes.footer}>
                <LinkButton
                  dataTestId="clear-all-notifications-btn"
                  onClick={localHandleClearAllNotifications}
                >
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
