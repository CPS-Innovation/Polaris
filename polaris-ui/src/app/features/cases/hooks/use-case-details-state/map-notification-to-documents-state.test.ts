import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  NotificationEventCore,
  NotificationReasonMap,
  NotificationState,
} from "../../domain/NotificationState";
import { mapNotificationToDocumentsState } from "./map-notification-to-documents-state";

type TDocumentsState = Parameters<typeof mapNotificationToDocumentsState>[1];

type TData = Exclude<TDocumentsState, { status: "loading" }>["data"];

type TNotificationState = NotificationState<NotificationEventCore>;

describe("mapNotificationToDocumentsState", () => {
  it("should not touch documentsState if it is still loading", () => {
    // Arrange
    const notificationsState: TNotificationState = {
      events: [],
      ignoreNextEvents: [],
    };
    const documentsState: TDocumentsState = {
      status: "loading",
    };

    // Act
    const result = mapNotificationToDocumentsState(
      notificationsState,
      documentsState
    );

    // Assert
    expect(result).toBe(documentsState);
  });

  it.each<[string, TNotificationState["events"], TData, TData]>([
    [
      "should update a documents with empty tags if a new event has occurred",
      [
        { documentId: "1", reason: "New" },
        { documentId: "2", reason: "New Version" },
      ],
      [
        { documentId: "1", tags: [] },
        {
          documentId: "2",
          tags: [
            {
              label: "New Version",
              color: NotificationReasonMap["New Version"],
            },
          ],
        },
      ],
      [
        {
          documentId: "1",
          tags: [{ label: "New", color: NotificationReasonMap["New"] }],
        },
        {
          documentId: "2",
          tags: [
            {
              label: "New Version",
              color: NotificationReasonMap["New Version"],
            },
          ],
        },
      ],
    ],
    [
      "should update a documents with exiting tags if a new event has occurred",
      [
        { documentId: "1", reason: "New" },
        { documentId: "1", reason: "Updated" },
      ],
      [
        {
          documentId: "1",
          tags: [{ label: "New", color: NotificationReasonMap["New"] }],
        },
      ],
      [
        {
          documentId: "1",
          tags: [
            { label: "New", color: NotificationReasonMap["New"] },
            { label: "Updated", color: NotificationReasonMap["Updated"] },
          ],
        },
      ],
    ],
    [
      "should update a documents with exiting tags if an event has been cleared",
      [{ documentId: "1", reason: "New" }],
      [
        {
          documentId: "1",
          tags: [
            { label: "New", color: NotificationReasonMap["New"] },
            { label: "Updated", color: NotificationReasonMap["Updated"] },
          ],
        },
      ],
      [
        {
          documentId: "1",
          tags: [{ label: "New", color: NotificationReasonMap["New"] }],
        },
      ],
    ],
    [
      "should update a documents with exiting tags if all events have been cleared",
      [],
      [
        {
          documentId: "1",
          tags: [
            { label: "New", color: NotificationReasonMap["New"] },
            { label: "Updated", color: NotificationReasonMap["Updated"] },
          ],
        },
      ],
      [
        {
          documentId: "1",
          tags: [],
        },
      ],
    ],
    [
      "should update a documents with when tags are added and removed",
      [
        { documentId: "1", reason: "Reclassified" },
        { documentId: "1", reason: "New" },
      ],
      [
        {
          documentId: "1",
          tags: [
            { label: "New", color: NotificationReasonMap["New"] },
            {
              label: "New Version",
              color: NotificationReasonMap["New Version"],
            },
          ],
        },
      ],
      [
        {
          documentId: "1",
          tags: [
            { label: "New", color: NotificationReasonMap["New"] },
            {
              label: "Reclassified",
              color: NotificationReasonMap["Reclassified"],
            },
          ],
        },
      ],
    ],
  ])("%s", (_, inputEvents, inputDocsState, expectedDocsState) => {
    // Arrange
    const notificationsState: TNotificationState = {
      events: inputEvents,
      ignoreNextEvents: [],
    };

    // Act
    const result = mapNotificationToDocumentsState(notificationsState, {
      status: "succeeded",
      data: inputDocsState,
    });

    // Assert
    expect(result).toEqual({ status: "succeeded", data: expectedDocsState });
  });
});