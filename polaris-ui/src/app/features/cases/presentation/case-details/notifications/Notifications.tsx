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
    ({ documentId, mode }: { documentId: string; mode: "read" | "search" }) => {
      trackEvent("Document Opened from Notification", { documentId });
      handleOpenPdf({ documentId, mode });
    },
    [handleOpenPdf, trackEvent]
  );

  const localHandleClearNotification = useCallback(
    (notificationId: number, documentId: string) => {
      trackEvent("Notification Cleared", { notificationId, documentId });
      handleClearNotification(notificationId);
    },
    [handleClearNotification, trackEvent]
  );

  const localHandleClearAllNotifications = useCallback(() => {
    trackEvent("All Notifications Cleared");
    handleClearAllNotifications();
  }, [handleClearAllNotifications, trackEvent]);

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
            {lastCheckedDateTime ? (
              <Time dateTime={lastCheckedDateTime} />
            ) : (
              "please wait..."
            )}
          </div>
          {!!eventsToDisplay.length && (
            <div className={classes.body}>
              <ul ref={listRef} onScroll={handleScroll}>
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
              <div className={classes.footer}>
                <LinkButton onClick={localHandleClearAllNotifications}>
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
