export type NotificationType =
  | "New"
  | "Discarded"
  | "NewVersion"
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
  narrative: undefined;
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
