type NotificationType =
  | "New"
  | "Discarded"
  | "NewVersion"
  | "Reclassified"
  | "Updated"
  | "Superseded";

type NotificationEventCore = {
  documentId: string;
  cmsVersionId: number;
  notificationType: NotificationType;
};

type NotificationEventMetaData = {
  presentationTitle: string;
  actioned: boolean;
  dateTime: string;
  narrative: string;
};

type NotificationEvent = NotificationEventCore & NotificationEventMetaData;

export type NotificationState = {
  lastUpdatedDateTime?: string;
  ignoreNextEvents: NotificationEventCore[];
  events: NotificationEvent[];
};
