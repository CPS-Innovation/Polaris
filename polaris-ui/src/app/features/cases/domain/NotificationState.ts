export type NotificationType =
  | "New"
  | "Discarded"
  | "New Version"
  | "Reclassified"
  | "Updated";

type NotificationStatus = "Live" | "Read" | "Superseded";

export type NotificationEventCore = {
  documentId: string;
  notificationType: NotificationType;
};

export type NotificationEvent = NotificationEventCore & {
  id: number;
  cmsVersionId: number;
  presentationTitle: string;
  dateTime: string;
  narrative: string;
  status: NotificationStatus;
};

export type NotificationState = {
  liveNotificationCount: number;
  lastUpdatedDateTime?: string;
  ignoreNextEvents: NotificationEventCore[];
  events: NotificationEvent[];
};

export const buildDefaultNotificationState = () =>
  ({
    ignoreNextEvents: [],
    events: [],
    liveNotificationCount: 0,
  } as NotificationState);
