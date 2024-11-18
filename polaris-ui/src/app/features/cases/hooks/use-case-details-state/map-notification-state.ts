import { AsyncResult } from "../../../../common/types/AsyncResult";
import {
  BACKGROUND_PIPELINE_REFRESH_SHOW_OWN_NOTIFICATIONS,
  FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH,
} from "../../../../config";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  NotificationEvent,
  NotificationEventCore,
  NotificationState,
  NotificationReason,
} from "../../domain/NotificationState";

type Sense = "is same" | "is different";

const inLeftNotRight = <T extends U & { documentId: string }, U>(
  left: T[],
  right: U[],
  ...predicates: ((leftItem: T, rightItem: U) => boolean)[]
) =>
  left.filter(
    (leftItem) =>
      !right.some((rightItem) =>
        predicates.every((predicate) => predicate(leftItem, rightItem))
      )
  );

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
    .filter(([_, rightItem]) => rightItem !== undefined);

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

const sortBy =
  <T>(key: keyof T) =>
  (a: T, b: T) =>
    (a && a[key]) < (b && b[key]) ? -1 : (a && a[key]) > (b && b[key]) ? 1 : 0;

export const mapNotificationState = (
  notificationState: NotificationState,
  existingDocumentState: AsyncResult<MappedCaseDocument[]>,
  incomingDocumentsState: AsyncResult<MappedCaseDocument[]>,
  incomingDateTime: string
): NotificationState => {
  // if (!FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH) {
  //   return notificationState;
  // }

  if (incomingDocumentsState.status !== "succeeded") {
    return { ...notificationState, lastCheckedDateTime: incomingDateTime };
  }

  const existingDocuments =
    existingDocumentState.status === "succeeded"
      ? existingDocumentState.data
      : [];

  const incomingDocuments = incomingDocumentsState.data;

  // Every time we ask to generate an id we take the maximum id of the existing
  //  event array and add a counter to it (and increment the counter on each ask)
  let counter = 1;
  const generateNextId = () =>
    Math.max(0, ...notificationState.events.map((evt) => evt.id)) + counter++;

  // If this is first load then nothing to notify about - it is all new!
  const isFirstLoad = !existingDocuments.length;

  const buildEvent = (arg: {
    reason: NotificationReason;
    doc: MappedCaseDocument;
    oldDoc?: MappedCaseDocument;
  }): NotificationEvent => {
    const {
      doc: { documentId, versionId, presentationTitle, presentationCategory },
      reason,
    } = arg;

    const reasonToIgnore = isFirstLoad
      ? "First case load"
      : notificationState.ignoreNextEvents.some(
          (eventToIgnore) =>
            eventToIgnore.documentId === documentId &&
            eventToIgnore.reason === reason
        )
      ? "Users own event"
      : undefined;

    return {
      id: generateNextId(),
      documentId,
      versionId,
      reason,
      presentationTitle,
      dateTime: incomingDateTime,
      narrative: presentationCategory,
      status: "Live",
      reasonToIgnore,
    };
  };

  const newNotifications = inLeftNotRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same")
  ).map((doc) =>
    buildEvent({
      reason: "New",
      doc,
    })
  );

  const discardedNotifications = inLeftNotRight(
    existingDocuments,
    incomingDocuments,
    where("documentId", "is same")
  ).map((doc) => buildEvent({ reason: "Discarded", doc }));

  const newVersionNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same"),
    where("versionId", "is different")
  ).map(([doc, oldDoc]) => buildEvent({ reason: "New Version", doc, oldDoc }));

  const reclassifiedNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same"),
    whereNested("cmsDocType", "documentTypeId", "is different")
  ).map(([doc, oldDoc]) => buildEvent({ reason: "Reclassified", doc, oldDoc }));

  const updatedNotifications = inLeftAndRight(
    incomingDocuments,
    existingDocuments,
    where("documentId", "is same"),
    where("presentationTitle", "is different")
  ).map(([doc, oldDoc]) => buildEvent({ reason: "Updated", doc, oldDoc }));

  const incomingEvents: NotificationEvent[] = [
    ...newNotifications,
    ...discardedNotifications,
    ...newVersionNotifications,
    ...reclassifiedNotifications,
    ...updatedNotifications,
  ];

  // Any existing event that is for a document that has just been discarded needs to
  //  be set as Superseded
  const existingEventsStillLive = notificationState.events.map(
    (existingEvent) =>
      discardedNotifications.some(
        (discardedDoc) => discardedDoc.documentId === existingEvent.documentId
      ) && existingEvent.status !== "Superseded"
        ? ({ ...existingEvent, status: "Superseded" } as NotificationEvent)
        : existingEvent
  );

  // Remove any "ignore" event records that have been matched
  const ignoreNextEvents = inLeftNotRight(
    notificationState.ignoreNextEvents as NotificationEvent[],
    incomingEvents,
    where("documentId", "is same"),
    where("reason", "is same")
  );

  const events = [
    ...incomingEvents.sort(sortBy("documentId") || sortBy("reason")),
    ...existingEventsStillLive,
  ];

  const lastModifiedDateTime =
    !notificationState.lastModifiedDateTime || incomingEvents.length
      ? incomingDateTime
      : notificationState.lastModifiedDateTime;

  return {
    ...notificationState,
    lastCheckedDateTime: incomingDateTime,
    lastModifiedDateTime,
    events,
    ignoreNextEvents,
  };
};

export const registerNotifiableEvent = (
  state: NotificationState,
  event: NotificationEventCore
): NotificationState =>
  FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH
    ? {
        ...state,
        ignoreNextEvents: [...state.ignoreNextEvents, event],
      }
    : state;

export const readNotification = <
  T extends NotificationEventCore & Pick<NotificationEvent, "status" | "id">
>(
  state: NotificationState<T>,
  notificationId: number
): NotificationState<T> => {
  if (!FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH) {
    return state;
  }

  return state.events.some(
    (evt) => evt.id === notificationId && evt.status === "Live"
  )
    ? {
        ...state,
        events: state.events.map((evt) =>
          evt.id === notificationId ? { ...evt, status: "Read" } : evt
        ),
      }
    : state;
};

export const clearNotification = <
  T extends NotificationEventCore & Pick<NotificationEvent, "id">
>(
  state: NotificationState<T>,
  notificationId: number
): NotificationState<T> => {
  if (!FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH) {
    return state;
  }

  return {
    ...state,
    events: state.events.filter((evt) => evt.id !== notificationId),
  };
};

export const clearDocumentNotifications = <T extends NotificationEventCore>(
  state: NotificationState<T>,
  documentId: string
): NotificationState<T> => {
  if (!FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH) {
    return state;
  }

  return state.events.some((evt) => evt.documentId === documentId)
    ? {
        ...state,
        events: state.events.filter((evt) => evt.documentId !== documentId),
      }
    : state;
};

export const clearAllNotifications = <T extends NotificationEventCore>(
  state: NotificationState<T>
): NotificationState<T> => {
  if (!FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH) {
    return state;
  }

  return {
    ...state,
    events: [],
  };
};

export const filterNotificationsButtonEvents = <
  T extends Pick<NotificationEvent, "id" | "status" | "reasonToIgnore">
>(
  allEvents: T[]
) => {
  const eventsToDisplay = allEvents.filter(
    (evt) =>
      BACKGROUND_PIPELINE_REFRESH_SHOW_OWN_NOTIFICATIONS || !evt.reasonToIgnore
  );

  return {
    liveEventCount: eventsToDisplay.filter((evt) => evt.status === "Live")
      .length,
    eventsToDisplay,
  };
};

export const filterDocumentTagEvents = <
  T extends Pick<NotificationEvent, "reasonToIgnore">
>(
  allEvents: T[]
): T[] =>
  allEvents.filter(
    (evt) =>
      BACKGROUND_PIPELINE_REFRESH_SHOW_OWN_NOTIFICATIONS ||
      evt.reasonToIgnore !== "First case load"
  );
