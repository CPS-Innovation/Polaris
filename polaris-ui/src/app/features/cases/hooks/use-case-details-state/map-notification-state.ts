import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  NotificationEvent,
  NotificationEventCore,
  NotificationState,
  NotificationReason,
} from "../../domain/NotificationState";

type Sense = "is same" | "is different";

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

const where =
  <T extends U, U, Key extends keyof U>(key: Key, sense: Sense) =>
  (left: T, right: U) =>
    sense === "is same"
      ? (left && left[key]) === (right && right[key])
      : (left && left[key]) !== (right && right[key]);

const whereNested =
  <T extends U, U, Key extends keyof U, ChildKey extends keyof U[Key]>(
    key: Key,
    childKey: ChildKey,
    sense: Sense
  ) =>
  (left: T, right: U) =>
    sense === "is same"
      ? (left && left[key] && left[key][childKey]) ===
        (right && right[key] && right[key][childKey])
      : (left && left[key] && left[key][childKey]) !==
        (right && right[key] && right[key][childKey]);

const applyLiveNotificationCount = (
  notificationState: NotificationState
): NotificationState => {
  const liveNotificationCount = notificationState.events.filter(
    (evt) => evt.status === "Live"
  ).length;

  if (liveNotificationCount === notificationState.liveNotificationCount) {
    return notificationState;
  }

  return {
    ...notificationState,
    liveNotificationCount,
  };
};

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

  // Every time we ask to generate an id we take the maximum id of the existing
  //  event array and add a counter to it (and increment the counter on each ask)
  let counter = 1;
  const generateUniqueId = () =>
    Math.max(0, ...notificationState.events.map((evt) => evt.id)) + counter++;

  const buildEvent = (
    reason: NotificationReason,
    {
      documentId,
      cmsVersionId,
      presentationTitle,
      presentationCategory,
    }: MappedCaseDocument,
    oldDoc?: MappedCaseDocument
  ): NotificationEvent => ({
    id: generateUniqueId(),
    documentId,
    cmsVersionId,
    reason,
    presentationTitle,
    dateTime: incomingDateTime,
    narrative: presentationCategory,
    status: "Live",
  });

  const newNotifications = inLeftNotRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same")
  ).map((doc) => buildEvent("New", doc));

  const discardedNotifications = inLeftNotRight(
    existingDocuments,
    incomingDocuments,
    where("documentId", "is same")
  ).map((doc) => buildEvent("Discarded", doc));

  const newVersionNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same"),
    where("cmsVersionId", "is different")
  ).map(([doc, oldDoc]) => buildEvent("New Version", doc, oldDoc));

  const reclassifiedNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same"),
    whereNested("cmsDocType", "documentTypeId", "is different")
  ).map(([doc, oldDoc]) => buildEvent("Reclassified", doc, oldDoc));

  const updatedNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same"),
    where("presentationTitle", "is different")
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
    where("documentId", "is same"),
    where("reason", "is same")
  );

  const ignoreNextEventsNotMatchedThisTime = inLeftNotRight(
    notificationState.ignoreNextEvents as NotificationEvent[],
    incomingEvents,
    where("documentId", "is same"),
    where("reason", "is same")
  );

  const existingEvents = notificationState.events.map((existingEvent) =>
    discardedNotifications.some(
      (discardedDoc) => discardedDoc.documentId === existingEvent.documentId
    ) && existingEvent.status !== "Superseded"
      ? ({ ...existingEvent, status: "Superseded" } as NotificationEvent)
      : existingEvent
  );

  const events = [...eventsWithoutIgnored, ...existingEvents];

  const nextState = {
    ...notificationState,
    lastUpdatedDateTime: incomingDateTime,
    events,
    ignoreNextEvents: ignoreNextEventsNotMatchedThisTime,
  };

  return applyLiveNotificationCount(nextState);
};

export const registerNotifiableEvent = (
  state: NotificationState,
  event: NotificationEventCore
): NotificationState => ({
  ...state,
  ignoreNextEvents: [...state.ignoreNextEvents, event],
});

export const readNotification = (
  state: NotificationState | NotificationState,
  notificationId: number
): NotificationState => {
  const nextState = state.events.some(
    (evt) => evt.id === notificationId && evt.status === "Live"
  )
    ? {
        ...state,
        events: state.events.map((evt) =>
          evt.id === notificationId
            ? ({ ...evt, status: "Read" } as NotificationEvent)
            : evt
        ),
      }
    : state;

  return applyLiveNotificationCount(nextState);
};

export const clearNotification = (
  state: NotificationState,
  notificationId: number
): NotificationState => {
  const nextState = {
    ...state,
    events: state.events.filter((evt) => evt.id !== notificationId),
  };

  return applyLiveNotificationCount(nextState);
};

export const clearAllNotifications = (
  state: NotificationState
): NotificationState => {
  const nextState = {
    ...state,
    events: [],
  };

  return applyLiveNotificationCount(nextState);
};
