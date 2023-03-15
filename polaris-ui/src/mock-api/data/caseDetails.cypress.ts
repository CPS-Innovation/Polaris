import faker from "faker";
import { CaseDetails } from "../../app/features/cases/domain/CaseDetails";
import { CaseDetailsDataSource } from "./types/CaseDetailsDataSource";

const dataSource: CaseDetailsDataSource = (id) =>
  caseDetails.find((item) => item.id === id);

export default dataSource;

const caseDetails: CaseDetails[] = [
  {
    id: 13401,
    uniqueReferenceNumber: "12AB1111111",
    isCaseCharged: true,
    numberOfDefendants: 1,
    leadDefendantDetails: {
      id: 901,
      listOrder: 0,
      firstNames: "Steve",
      surname: "Walsh",
      organisationName: "",
      dob: "1977-11-28",
      isYouth: true,
      type: "SOME_TYPE",
    },
    headlineCharge: {
      charge: faker.lorem.sentence(),
      date: "2022-02-01",
      nextHearingDate: "2023-01-02",
    },
    defendants: [
      {
        defendantDetails: {
          id: 901,
          listOrder: 0,
          firstNames: "Steve",
          surname: "Walsh",
          organisationName: "",
          dob: "1977-11-28",
          isYouth: false,
          type: "SOME_TYPE",
        },
        custodyTimeLimit: {
          expiryDate: "2022-11-20",
          expiryDays: 20,
          expiryIndicator: "ACTIVE",
        },
        charges: [],
      },
    ],
  },
  {
    id: 13301,
    uniqueReferenceNumber: "12AB1111111",
    isCaseCharged: true,
    numberOfDefendants: 1,
    leadDefendantDetails: {
      id: 901,
      listOrder: 0,
      firstNames: "Steve",
      surname: "Walsh",
      organisationName: "",
      dob: "1977-11-28",
      isYouth: false,
      type: "SOME_TYPE",
    },
    headlineCharge: {
      charge: faker.lorem.sentence(),
      date: "2022-02-01",
      nextHearingDate: "2023-01-02",
    },
    defendants: [
      {
        defendantDetails: {
          id: 901,
          listOrder: 0,
          firstNames: "Steve",
          surname: "Walsh",
          organisationName: "",
          dob: "1977-11-28",
          isYouth: false,
          type: "SOME_TYPE",
        },
        custodyTimeLimit: {
          expiryDate: "2022-11-20",
          expiryDays: 20,
          expiryIndicator: "ACTIVE",
        },
        charges: [],
      },
      {
        defendantDetails: {
          id: 902,
          listOrder: 2,
          firstNames: "Peter",
          surname: "Victor",
          organisationName: "",
          dob: "1987-10-20",
          isYouth: false,
          type: "SOME_TYPE",
        },
        custodyTimeLimit: {
          expiryDate: "2022-11-20",
          expiryDays: 120,
          expiryIndicator: "ACTIVE",
        },
        charges: [],
      },
      {
        defendantDetails: {
          id: 903,
          listOrder: 1,
          firstNames: "Scott",
          surname: "Taylor",
          organisationName: "",
          dob: "1980-08-22",
          isYouth: false,
          type: "SOME_TYPE",
        },
        custodyTimeLimit: {
          expiryDate: "2022-11-20",
          expiryDays: 180,
          expiryIndicator: "ACTIVE",
        },
        charges: [],
      },
    ],
  },

  {
    id: 13201,
    uniqueReferenceNumber: "12AB1111111",
    isCaseCharged: true,
    numberOfDefendants: 1,
    leadDefendantDetails: {
      id: 901,
      listOrder: 0,
      firstNames: "Steve",
      surname: "Walsh",
      organisationName: "",
      dob: "1977-11-28",
      isYouth: false,
      type: "SOME_TYPE",
    },
    headlineCharge: {
      charge: faker.lorem.sentence(),
      date: "2022-02-01",
      nextHearingDate: "2023-01-02",
    },
    defendants: [
      {
        defendantDetails: {
          id: 901,
          listOrder: 0,
          firstNames: "Steve",
          surname: "Walsh",
          organisationName: "",
          dob: "1977-11-28",
          isYouth: false,
          type: "SOME_TYPE",
        },
        custodyTimeLimit: {
          expiryDate: "2022-11-20",
          expiryDays: 20,
          expiryIndicator: "ACTIVE",
        },
        charges: [
          {
            id: 1,
            listOrder: 1,
            isCharged: true,
            nextHearingDate: "2022-11-20",
            earlyDate: "2022-11-10",
            lateDate: "2022-11-27",
            code: "abc",
            shortDescription: "short description",
            longDescription: "long description",
            custodyTimeLimit: {
              expiryDate: "2022-11-20",
              expiryDays: 20,
              expiryIndicator: "ACTIVE",
            },
          },
          {
            id: 2,
            listOrder: 2,
            isCharged: true,
            nextHearingDate: "2022-11-20",
            earlyDate: "2022-11-10",
            lateDate: "2022-11-27",
            code: "abc",
            shortDescription: "short description",
            longDescription: "long description",
            custodyTimeLimit: {
              expiryDate: "2022-11-20",
              expiryDays: 20,
              expiryIndicator: "ACTIVE",
            },
          },
        ],
      },
    ],
  },
];
