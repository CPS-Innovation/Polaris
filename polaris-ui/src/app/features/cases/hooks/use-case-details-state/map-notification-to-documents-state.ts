import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  NotificationEvent,
  NotificationEventCore,
  NotificationReasonMap,
  NotificationState,
} from "../../domain/NotificationState";

const sortBy =
  <T>(key: keyof T) =>
  (a: T, b: T) =>
    (a && a[key]) < (b && b[key]) ? -1 : (a && a[key]) > (b && b[key]) ? 1 : 0;

export const mapNotificationToDocumentsState = <
  // make the typing as accommodating as possible for unit tests
  T extends NotificationEventCore = NotificationEvent,
  U extends Pick<MappedCaseDocument, "documentId" | "tags"> = MappedCaseDocument
>(
  notificationState: NotificationState<T>,
  documentsState: AsyncResult<U[]>
): AsyncResult<U[]> => {
  if (documentsState.status !== "succeeded") {
    return documentsState;
  }

  let hasAnyDocBeenUpdated = false;
  const data = documentsState.data.map((doc) => {
    const nextTagsForDoc = notificationState.events
      .filter((evt) => evt.documentId === doc.documentId)
      .map(
        (evt) =>
          ({
            label: evt.reason,
            color: NotificationReasonMap[evt.reason],
          } as MappedCaseDocument["tags"][0])
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
