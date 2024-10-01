import { TagColor } from "../../../common/presentation/types/TagColor";

export type NotificationReason = keyof typeof NotificationReasonMap;

// note if the colours do not overlap with the colours expected by our
//  Tag control then we will get a compiler error (which is what we want)
export const NotificationReasonMap: Record<string, TagColor> = {
  New: "orange", // documentId has appeared in the tracker
  Discarded: "orange", // documentId has disappeared from the tracker
  "New Version": "orange", // documentId has changed version
  Reclassified: "orange", // docTypeId change
  Updated: "orange", // presentationTitle change
};

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
  reasonToIgnore?: "First case load" | "Users own event";
};

export type NotificationState<
  T extends NotificationEventCore = NotificationEvent
> = {
  lastUpdatedDateTime?: string;
  // ignoreNextEvents allows us to register events triggered by the current user
  //  so that we may ignore them when we see the corresponding document change
  //  in the tracker
  ignoreNextEvents: NotificationEventCore[];
  events: T[];
};

export const buildDefaultNotificationState = <
  T extends NotificationEventCore
>() =>
  ({
    ignoreNextEvents: [],
    events: [],
  } as NotificationState<T>);
