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
  cmsVersionId: number;
  presentationTitle: string;
  dateTime: string;
  narrative: undefined;
  status: NotificationStatus;
};

export type NotificationState = {
  lastUpdatedDateTime?: string;
  ignoreNextEvents: NotificationEventCore[];
  events: NotificationEvent[];
};
