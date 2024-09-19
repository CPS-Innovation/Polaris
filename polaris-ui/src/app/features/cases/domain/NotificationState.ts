export type NotificationType =
  | "New"
  | "Discarded"
  | "NewVersion"
  | "Reclassified"
  | "Updated";

type NotificationStatus = "Live" | "Read" | "Superseded";

type NotificationEventCore = {
  documentId: string;

  notificationType: NotificationType;
};

type NotificationEventMetaData = {
  cmsVersionId: number;
  presentationTitle: string;
  dateTime: string;
  narrative: undefined;
  status: NotificationStatus;
};

export type NotificationEvent = NotificationEventCore &
  NotificationEventMetaData;

export type NotificationState = {
  lastUpdatedDateTime?: string;
  ignoreNextEvents: NotificationEventCore[];
  events: NotificationEvent[];
};
