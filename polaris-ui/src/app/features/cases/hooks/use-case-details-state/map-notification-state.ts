import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  NotificationEvent,
  NotificationState,
  NotificationType,
} from "../../domain/NotificationState";

type Sense = "same" | "different";

const sortFn = (a: string, b: string) => (a > b ? 1 : a < b ? -1 : 0);

const inLeftNotRight = <T extends U & { documentId: string }, U>(
  left: T[],
  right: U[],
  ...predicates: ((leftItem: T, rightItem: U) => boolean)[]
) =>
  left
    .filter(
      (leftItem) =>
        !right.some((rightItem) =>
          predicates.every((predicate) => predicate(leftItem, rightItem))
        )
    )
    // a deterministic ordering is useful for tests
    .sort((a, b) => sortFn(a.documentId, b.documentId));

const inLeftAndRight = <T extends { documentId: string }>(
  left: T[],
  right: T[],
  ...predicates: ((leftItem: T, rightItem: T) => boolean)[]
) =>
  left
    .map((leftItem) => [
      leftItem,
      right.find((rightItem) =>
        predicates.every((predicate) => predicate(leftItem, rightItem))
      )!,
    ])
    .filter(([_, rightItem]) => rightItem !== undefined)
    // a deterministic ordering is useful for tests
    .sort(([a], [b]) => sortFn(a.documentId, b.documentId));

const match =
  <T extends U, U, Key extends keyof U>(key: Key, sense: Sense) =>
  (left: T, right: U) =>
    sense === "same"
      ? (left && left[key]) === (right && right[key])
      : (left && left[key]) !== (right && right[key]);

const matchNested =
  <T extends U, U, Key extends keyof U, ChildKey extends keyof U[Key]>(
    key: Key,
    childKey: ChildKey,
    sense: Sense
  ) =>
  (left: T, right: U) =>
    sense === "same"
      ? (left && left[key] && left[key][childKey]) ===
        (right && right[key] && right[key][childKey])
      : (left && left[key] && left[key][childKey]) !==
        (right && right[key] && right[key][childKey]);

export const mapNotificationState = (
  notificationState: NotificationState,
  existingDocuments: MappedCaseDocument[] = [],
  incomingDocuments: MappedCaseDocument[] = [],
  incomingDateTime: string
): NotificationState => {
  if (!existingDocuments.length) {
    // If this is first load then nothing to notify about - it is all new!
    return { ...notificationState, lastUpdatedDateTime: incomingDateTime };
  }

  const buildEvent = (
    notificationType: NotificationType,
    {
      documentId,
      cmsVersionId,
      presentationTitle,
      cmsDocType: { documentTypeId },
    }: MappedCaseDocument,
    oldDoc?: MappedCaseDocument
  ): NotificationEvent => ({
    documentId,
    cmsVersionId,
    notificationType,
    presentationTitle,
    dateTime: incomingDateTime,
    narrative: undefined,
    status:
      notificationType === "Discarded"
        ? // if it is Discarded then we do not need the Notification to be clickable
          "Read"
        : "Live",
  });

  const newNotifications = inLeftNotRight(
    incomingDocuments,
    existingDocuments,
    match("documentId", "same")
  ).map((doc) => buildEvent("New", doc));

  const discardedNotifications = inLeftNotRight(
    existingDocuments,
    incomingDocuments,
    match("documentId", "same")
  ).map((doc) => buildEvent("Discarded", doc));

  const newVersionNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    match("documentId", "same"),
    match("cmsVersionId", "different")
  ).map(([doc, oldDoc]) => buildEvent("NewVersion", doc, oldDoc));

  const reclassifiedNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    match("documentId", "same"),
    matchNested("cmsDocType", "documentTypeId", "different")
  ).map(([doc, oldDoc]) => buildEvent("Reclassified", doc, oldDoc));

  const updatedNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    match("documentId", "same"),
    match("presentationTitle", "different")
  ).map(([doc, oldDoc]) => buildEvent("Updated", doc, oldDoc));

  const incomingEvents: NotificationEvent[] = [
    ...newNotifications,
    ...discardedNotifications,
    ...newVersionNotifications,
    ...reclassifiedNotifications,
    ...updatedNotifications,
  ];

  const eventsWithoutIgnored = inLeftNotRight(
    incomingEvents,
    notificationState.ignoreNextEvents,
    match("documentId", "same"),
    match("notificationType", "same")
  );

  const ignoreNextEventsStillUnmatched = inLeftNotRight(
    notificationState.ignoreNextEvents as NotificationEvent[],
    incomingEvents,
    match("documentId", "same"),
    match("notificationType", "same")
  );

  const existingEvents = notificationState.events.map((existingEvent) =>
    existingEvent.status !== "Superseded" &&
    discardedNotifications.some(
      (discardedDoc) => discardedDoc.documentId === existingEvent.documentId
    )
      ? ({ ...existingEvent, status: "Superseded" } as NotificationEvent)
      : existingEvent
  );

  const nextState = {
    ...notificationState,
    lastUpdatedDateTime: incomingDateTime,
    events: [...eventsWithoutIgnored, ...existingEvents],
    ignoreNextEvents: ignoreNextEventsStillUnmatched,
  };

  console.debug(notificationState, nextState);

  return nextState;
};
