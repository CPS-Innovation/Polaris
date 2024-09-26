import { CmsDocType } from "../../domain/gateway/CmsDocType";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  NotificationEvent,
  NotificationState,
} from "../../domain/NotificationState";
import { mapNotificationState } from "./map-notification-state";

const existingDateTime = "2024-09-18T14:00:00Z";
const incomingDateTime = "2024-09-18T14:33:33Z";

type Scenario = [
  string,
  {
    existing: [NotificationState, MappedCaseDocument[] | undefined];
    incoming: [MappedCaseDocument[] | undefined, string];
    expected: NotificationState;
  }
];

const doc = (
  doc: Partial<
    Omit<MappedCaseDocument, "cmsDocType"> & { cmsDocType: Partial<CmsDocType> }
  >
) => ({ cmsDocType: {}, ...doc } as MappedCaseDocument);
const state = (state: Partial<NotificationState>) =>
  ({ events: [], ignoreNextEvents: [], ...state } as NotificationState);
const evt = (notification: Partial<NotificationEvent>) =>
  notification as NotificationEvent;

const scenarios: Scenario[] = [
  [
    "will not add notifications if empty existing documents (is the first load of the data)",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
        }),
        [],
      ],
      incoming: [[doc({ documentId: "1" })], incomingDateTime],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
      }),
    },
  ],
  [
    "will not add notifications if undefined existing documents (is the first load of the data)",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
        }),
        undefined,
      ],
      incoming: [[doc({ documentId: "1" })], incomingDateTime],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
      }),
    },
  ],
  [
    "will not throw on undefined cmsDocType nested objects on documents",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
        }),
        [doc({ documentId: "1", cmsDocType: undefined })],
      ],
      incoming: [
        [doc({ documentId: "1", cmsDocType: undefined })],
        incomingDateTime,
      ],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        liveNotificationCount: 0,
      }),
    },
  ],
  [
    "will add New notifications for new documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0", id: 0 })],
        }),
        [doc({ documentId: "0" })],
      ],
      incoming: [
        [
          doc({
            documentId: "2",
            cmsVersionId: 22,
            presentationTitle: "doc-2",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
            presentationTitle: "doc-1",
          }),
        ],
        incomingDateTime,
      ],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "1",
            cmsVersionId: 11,
            reason: "New",
            presentationTitle: "doc-1",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            cmsVersionId: 22,
            reason: "New",
            presentationTitle: "doc-2",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({ documentId: "0", id: 0 }),
        ],
        liveNotificationCount: 2,
      }),
    },
  ],
  [
    "will add Discarded notifications for deleted documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0", id: 0 })],
        }),
        [
          doc({
            documentId: "2",
            cmsVersionId: 22,
            presentationTitle: "doc-2",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
            presentationTitle: "doc-1",
          }),
        ],
      ],
      incoming: [[doc({ documentId: "0" })], incomingDateTime],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "1",
            cmsVersionId: 11,
            reason: "Discarded",
            presentationTitle: "doc-1",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            cmsVersionId: 22,
            reason: "Discarded",
            presentationTitle: "doc-2",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({ documentId: "0", id: 0 }),
        ],
        liveNotificationCount: 2,
      }),
    },
  ],
  [
    "will add New Version notifications for upversioned documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0", id: 0 })],
        }),
        [
          doc({
            documentId: "2",
            cmsVersionId: 22,
            presentationTitle: "doc-2-22",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
            presentationTitle: "doc-1-11",
          }),
        ],
      ],
      incoming: [
        [
          doc({
            documentId: "2",
            cmsVersionId: 222,
            presentationTitle: "doc-2-22",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 111,
            presentationTitle: "doc-1-11",
          }),
        ],
        incomingDateTime,
      ],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "1",
            cmsVersionId: 111,
            presentationTitle: "doc-1-11",
            reason: "New Version",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            presentationTitle: "doc-2-22",
            reason: "New Version",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({ documentId: "0", id: 0 }),
        ],
        liveNotificationCount: 2,
      }),
    },
  ],
  [
    "will add Reclassified notifications for reclassified documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0", id: 0 })],
        }),
        [
          doc({
            documentId: "2",
            cmsDocType: {
              documentTypeId: 22,
            },
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsDocType: {
              documentTypeId: 11,
            },
          }),
        ],
      ],
      incoming: [
        [
          doc({
            documentId: "2",
            cmsDocType: {
              documentTypeId: 222,
            },
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsDocType: {
              documentTypeId: 111,
            },
          }),
        ],
        incomingDateTime,
      ],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "1",
            reason: "Reclassified",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            reason: "Reclassified",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({ documentId: "0", id: 0 }),
        ],
        liveNotificationCount: 2,
      }),
    },
  ],
  [
    "will add Updated notifications for updated documents to the top of the notification list ",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0", id: 0 })],
        }),
        [
          doc({
            documentId: "2",
            presentationTitle: "doc-2",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            presentationTitle: "doc-1",
          }),
        ],
      ],
      incoming: [
        [
          doc({
            documentId: "2",
            presentationTitle: "doc-2222",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            presentationTitle: "doc-1111",
          }),
        ],
        incomingDateTime,
      ],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "1",
            reason: "Updated",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            reason: "Updated",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({ documentId: "0", id: 0 }),
        ],
        liveNotificationCount: 2,
      }),
    },
  ],
  [
    "will add New Version, Reclassified and Updated notifications for documents that have changed all three respects to the top of the notification list ",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0", id: 0 })],
        }),
        [
          doc({
            documentId: "2",
            cmsVersionId: 22,
            cmsDocType: {
              documentTypeId: 22,
            },
            presentationTitle: "doc-2",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
            cmsDocType: {
              documentTypeId: 11,
            },
            presentationTitle: "doc-1",
          }),
        ],
      ],
      incoming: [
        [
          doc({
            documentId: "2",
            cmsVersionId: 222,
            cmsDocType: {
              documentTypeId: 222,
            },
            presentationTitle: "doc-2222",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 111,
            cmsDocType: {
              documentTypeId: 111,
            },
            presentationTitle: "doc-1111",
          }),
        ],
        incomingDateTime,
      ],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "1",
            cmsVersionId: 111,
            reason: "New Version",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "1",
            cmsVersionId: 111,
            reason: "Reclassified",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "1",
            cmsVersionId: 111,
            reason: "Updated",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            reason: "New Version",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            reason: "Reclassified",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            reason: "Updated",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({ documentId: "0", id: 0 }),
        ],
        liveNotificationCount: 6,
      }),
    },
  ],
  [
    "will ignore events that the user has themselves initiated and remove the ignore record for notifications that have been found and ignored",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [
            evt({
              documentId: "0",
              status: "Live",
              id: 0,
            }),
          ],
          ignoreNextEvents: [
            evt({ documentId: "3", reason: "New" }),
            evt({ documentId: "5", reason: "New Version" }),
          ],
        }),
        [
          doc({ documentId: "0" }),
          doc({
            documentId: "5",
            cmsVersionId: 55,
            cmsDocType: {
              documentTypeId: 5,
            },
            presentationTitle: "doc-5",
          }),
        ],
      ],
      incoming: [
        [
          doc({
            documentId: "5",
            cmsVersionId: 555,
            cmsDocType: {
              documentTypeId: 55,
            },
            presentationTitle: "doc-555",
          }),
          doc({
            documentId: "4",
            cmsVersionId: 44,
            presentationTitle: "doc-4",
          }),
          doc({ documentId: "0" }),
        ],
        incomingDateTime,
      ],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "4",
            cmsVersionId: 44,
            reason: "New",
            presentationTitle: "doc-4",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "5",
            cmsVersionId: 555,
            reason: "Reclassified",
            presentationTitle: "doc-555",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "5",
            cmsVersionId: 555,
            reason: "Updated",
            presentationTitle: "doc-555",
            dateTime: incomingDateTime,
            status: "Live",
            id: expect.any(Number),
          }),
          evt({
            documentId: "0",
            status: "Live",
            id: 0,
          }),
        ],
        ignoreNextEvents: [evt({ documentId: "3", reason: "New" })],
        liveNotificationCount: 4,
      }),
    },
  ],
  [
    "will supersede any existing notifications where a more recent Discarded notification has been created for the relevant document",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [
            evt({
              documentId: "1",
              reason: "Reclassified",
              status: "Live",
              id: 1,
            }),
            evt({
              documentId: "1",
              reason: "Updated",
              status: "Live",
              id: 2,
            }),
            evt({
              documentId: "0",
              reason: "Reclassified",
              status: "Live",
              id: 0,
            }),
          ],
        }),
        [
          doc({ documentId: "0" }),
          doc({ documentId: "1", presentationTitle: "doc-1-11" }),
        ],
      ],
      incoming: [[doc({ documentId: "0" })], incomingDateTime],
      expected: state({
        lastUpdatedDateTime: incomingDateTime,
        events: [
          evt({
            documentId: "1",
            reason: "Discarded",
            status: "Live",
            presentationTitle: "doc-1-11",
            dateTime: incomingDateTime,
            id: expect.any(Number),
          }),
          evt({
            documentId: "1",
            reason: "Reclassified",
            status: "Superseded",
            id: 1,
          }),
          evt({
            documentId: "1",
            reason: "Updated",
            status: "Superseded",
            id: 2,
          }),
          evt({
            documentId: "0",
            reason: "Reclassified",
            status: "Live",
            id: 0,
          }),
        ],
        liveNotificationCount: 2,
      }),
    },
  ],
];

const toFlatScenario = (s: Scenario) =>
  [
    s[0],
    s[1].existing[0],
    s[1].existing[1],
    s[1].incoming[0],
    s[1].incoming[1],
    s[1].expected,
  ] as const;

describe("mapNotificationState", () => {
  beforeEach(() => {
    jest.spyOn(console, "debug").mockImplementation(jest.fn());
  });

  it.each<ReturnType<typeof toFlatScenario>>(scenarios.map(toFlatScenario))(
    "%s",
    (
      description,
      existingNotificationState,
      existingDocuments,
      incomingDocuments,
      incomingDateTime,
      expectedNotificationState
    ) => {
      // Act
      const result = mapNotificationState(
        existingNotificationState,
        existingDocuments,
        incomingDocuments,
        incomingDateTime
      );

      // Assert
      expect(result).toEqual(expectedNotificationState);

      // We expect whatever id generation mechanism is being used to generate unique ids
      const eventualNotificationIds = result.events.map(({ id }) => id);
      const uniqueNotificationIds = [...new Set(eventualNotificationIds)];
      expect(eventualNotificationIds.length).toEqual(
        uniqueNotificationIds.length
      );
    }
  );
});
