import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  NotificationEvent,
  NotificationEventCore,
  NotificationReason,
  NotificationReasonMap,
  NotificationState,
} from "../../domain/NotificationState";
import { TagType } from "../../domain/TagType";
import { filterDocumentTagEvents } from "./map-notification-state";

const sortBy =
  <T>(key: keyof T) =>
  (a: T, b: T) =>
    (a && a[key]) < (b && b[key]) ? -1 : (a && a[key]) > (b && b[key]) ? 1 : 0;

type TMinimalNotificationEvent = NotificationEventCore &
  Pick<NotificationEvent, "reasonToIgnore">;

export const mapNotificationToDocumentsState = <
  // make the typing as accommodating as possible for unit tests
  T extends TMinimalNotificationEvent = NotificationEvent,
  U extends Pick<MappedCaseDocument, "documentId" | "tags"> = MappedCaseDocument
>(
  notificationState: NotificationState<T>,
  documentsState: AsyncResult<U[]>
): AsyncResult<U[]> => {
  if (documentsState.status !== "succeeded") {
    return documentsState;
  }

  let hasAnyDocBeenUpdated = false;

  const events = filterDocumentTagEvents(notificationState.events);

  const data = documentsState.data.map((doc) => {
    const nextTagsForDoc = events
      .filter((evt) => evt.documentId === doc.documentId)
      // we want unique tags, e.g. New, Updated not New, Updated, Updated
      .reduce<NotificationReason[]>(
        (prev, curr) =>
          prev.some((reason) => reason === curr.reason)
            ? prev
            : [...prev, curr.reason],
        []
      )
      .map(
        (reason) =>
          <TagType>{ label: reason, color: NotificationReasonMap[reason] }
      )
      .sort(sortBy("label"));

    const currentTags = doc.tags.sort(sortBy("label"));

    const isChanged =
      nextTagsForDoc.length !== currentTags.length ||
      !nextTagsForDoc.every(
        (nextTag, i) =>
          currentTags[i].label === nextTag.label &&
          currentTags[i].color === nextTag.color
      );

    hasAnyDocBeenUpdated = hasAnyDocBeenUpdated || isChanged;

    return isChanged ? { ...doc, tags: nextTagsForDoc } : doc;
  });

  return hasAnyDocBeenUpdated
    ? {
        ...documentsState,
        data,
      }
    : documentsState;
};
