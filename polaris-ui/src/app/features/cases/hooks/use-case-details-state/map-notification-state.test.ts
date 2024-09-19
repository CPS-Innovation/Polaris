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
    "Will add New notifications for new documents to the top of the notification list",
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
    "Will add Discarded notifications for deleted documents to the top of the notification list",
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
    "Will add New version notifications for upversioned documents to the top of the notification list and prefer NewVersion over Reclassified and Updated classifications",
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
            cmsDocType: {
              documentTypeId: 222,
            },
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
            presentationTitle: "doc-1",
            cmsDocType: {
              documentTypeId: 111,
            },
          }),
        ],
      ],
      incoming: [
        [
          doc({
            documentId: "2",
            cmsVersionId: 222,
            presentationTitle: "doc-2222",
            cmsDocType: {
              documentTypeId: 2222,
            },
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 111,
            presentationTitle: "doc-1111",
            cmsDocType: {
              documentTypeId: 1111,
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
            cmsVersionId: 111,
            notificationType: "NewVersion",
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
          evt({ documentId: "0" }),
        ],
      }),
    },
  ],
  [
    "Will add Reclassified notifications for reclassified documents to the top of the notification list and prefer Reclassified over Updated classifications",
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
            cmsDocType: {
              documentTypeId: 222,
            },
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
            presentationTitle: "doc-1",
            cmsDocType: {
              documentTypeId: 111,
            },
          }),
        ],
      ],
      incoming: [
        [
          doc({
            documentId: "2",
            cmsVersionId: 22,
            presentationTitle: "doc-2222",
            cmsDocType: {
              documentTypeId: 2222,
            },
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
            presentationTitle: "doc-1111",
            cmsDocType: {
              documentTypeId: 1111,
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
            cmsVersionId: 11,
            notificationType: "Reclassified",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 22,
            notificationType: "Reclassified",
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
    "Will add Updated notifications for updated documents to the top of the notification list ",
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
      incoming: [
        [
          doc({
            documentId: "2",
            cmsVersionId: 22,
            presentationTitle: "doc-2222",
          }),
          doc({ documentId: "0" }),
          doc({
            documentId: "1",
            cmsVersionId: 11,
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
            cmsVersionId: 11,
            notificationType: "Updated",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 22,
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
    "Ignore events that the user has themselves initiated",
    {
      existing: [
        state({
          lastUpdatedDateTime: existingDateTime,
          events: [evt({ documentId: "0" })],
          ignoreNextEvents: [evt({ documentId: "3", notificationType: "New" })],
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
      incoming: [
        [
          doc({
            documentId: "3",
            cmsVersionId: 33,
            presentationTitle: "doc-3",
          }),
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
            notificationType: "Updated",
            presentationTitle: "doc-1111",
            dateTime: incomingDateTime,
            status: "Live",
          }),
          evt({
            documentId: "2",
            cmsVersionId: 22,
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
