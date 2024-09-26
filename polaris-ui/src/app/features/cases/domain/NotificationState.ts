export type NotificationReason =
  | "New" // documentId has appeared in the tracker
  | "Discarded" // documentId has disappeared from the tracker
  | "New Version" // documentId has changed version
  | "Reclassified" // docTypeId change
  | "Updated"; // presentationTitle change

export type NotificationEventCore = {
  documentId: string;
  reason: NotificationReason;
};

export type NotificationEvent = NotificationEventCore & {
  id: number;
  cmsVersionId: number;
  presentationTitle: string;
  dateTime: string;
  narrative: string;
  status: "Live" | "Read" | "Superseded";
  reasonToIgnore?: "First Case Load" | "Users own event";
};

export type NotificationState = {
  liveNotificationCount: number;
  lastUpdatedDateTime?: string;
  // ignoreNextEvents allows us to register events triggered by the current user
  //  so that we may ignore them when we see the corresponding document change
  //  in the tracker
  ignoreNextEvents: NotificationEventCore[];
  events: NotificationEvent[];
};

export const buildDefaultNotificationState = () =>
  ({
    ignoreNextEvents: [],
    events: [],
    liveNotificationCount: 0,
  } as NotificationState);
