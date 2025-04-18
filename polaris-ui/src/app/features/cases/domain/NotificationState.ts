import { TagColor } from "../../../common/presentation/types/TagColor";

export type NotificationReason =
  | "New"
  | "Discarded"
  | "New Version"
  | "Reclassified"
  | "Updated";

// note if the colours do not overlap with the colours expected by our
//  Tag control then we will get a compiler error (which is what we want)
export const NotificationReasonMap: Record<NotificationReason, TagColor> = {
  New: "green", // documentId has appeared in the tracker
  Discarded: "red", // documentId has disappeared from the tracker
  "New Version": "green", // documentId has changed version
  Reclassified: "purple", // docTypeId change
  Updated: "orange", // presentationTitle change
};

export type NotificationEventCore = {
  documentId: string;
  reason: NotificationReason;
};

export type NotificationEvent = NotificationEventCore & {
  id: number;
  versionId: number;
  presentationTitle: string;
  dateTime: string;
  narrative: string;
  status: "Live" | "Read" | "Superseded";
  reasonToIgnore?: "First case load" | "Users own event";
};

export type NotificationState<
  T extends NotificationEventCore = NotificationEvent
> = {
  lastCheckedDateTime?: string;
  lastModifiedDateTime?: string;
  // ignoreNextEvents allows us to register events triggered by the current user
  //  so that we may ignore them when we see the corresponding document change
  //  in the tracker
  ignoreNextEvents: NotificationEventCore[];
  events: T[];
};

export const buildDefaultNotificationState = <
  T extends NotificationEventCore
>(): NotificationState<T> => ({
  ignoreNextEvents: [],
  events: [],
});
