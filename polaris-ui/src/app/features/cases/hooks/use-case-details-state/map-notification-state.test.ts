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
    "will add New notifications for new documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0" })],
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
            notificationType: "New",
            presentationTitle: "doc-1",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 22,
            notificationType: "New",
            presentationTitle: "doc-2",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({ documentId: "0" }),
        ],
      }),
    },
  ],
  [
    "will add Discarded notifications for deleted documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0" })],
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
            notificationType: "Discarded",
            presentationTitle: "doc-1",
            dateTime: incomingDateTime,
            status: "Read",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 22,
            notificationType: "Discarded",
            presentationTitle: "doc-2",
            dateTime: incomingDateTime,
            status: "Read",
          }),
          evt({ documentId: "0" }),
        ],
      }),
    },
  ],
  [
    "will add NewVersion notifications for upversioned documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0" })],
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
            notificationType: "NewVersion",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            presentationTitle: "doc-2-22",
            notificationType: "NewVersion",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({ documentId: "0" }),
        ],
      }),
    },
  ],
  [
    "will add Reclassified notifications for reclassified documents to the top of the notification list",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0" })],
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
            notificationType: "Reclassified",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            notificationType: "Reclassified",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({ documentId: "0" }),
        ],
      }),
    },
  ],
  [
    "will add Updated notifications for updated documents to the top of the notification list ",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0" })],
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
            notificationType: "Updated",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            notificationType: "Updated",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({ documentId: "0" }),
        ],
      }),
    },
  ],
  [
    "will add NewVersion, Reclassified and Updated notifications for documents that have changed all three respects to the top of the notification list ",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0" })],
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
            notificationType: "NewVersion",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "1",
            cmsVersionId: 111,
            notificationType: "Reclassified",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "1",
            cmsVersionId: 111,
            notificationType: "Updated",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            notificationType: "NewVersion",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            notificationType: "Reclassified",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 222,
            notificationType: "Updated",
            presentationTitle: "doc-2222",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({ documentId: "0" }),
        ],
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
            }),
          ],
          ignoreNextEvents: [
            evt({ documentId: "3", notificationType: "New" }),
            evt({ documentId: "5", notificationType: "NewVersion" }),
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
            notificationType: "New",
            presentationTitle: "doc-4",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "5",
            cmsVersionId: 555,
            notificationType: "Reclassified",
            presentationTitle: "doc-555",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "5",
            cmsVersionId: 555,
            notificationType: "Updated",
            presentationTitle: "doc-555",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "0",
            status: "Live",
          }),
        ],
        ignoreNextEvents: [evt({ documentId: "3", notificationType: "New" })],
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
              notificationType: "Reclassified",
              status: "Live",
            }),
            evt({
              documentId: "1",
              notificationType: "Updated",
              status: "Live",
            }),
            evt({
              documentId: "0",
              notificationType: "Reclassified",
              status: "Live",
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
            notificationType: "Discarded",
            status: "Read",
            presentationTitle: "doc-1-11",
            dateTime: incomingDateTime,
          }),
          evt({
            documentId: "1",
            notificationType: "Reclassified",
            status: "Superseded",
          }),
          evt({
            documentId: "1",
            notificationType: "Updated",
            status: "Superseded",
          }),
          evt({
            documentId: "0",
            notificationType: "Reclassified",
            status: "Live",
          }),
        ],
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
    }
  );
});
