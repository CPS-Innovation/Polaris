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
          expiryIndicator: "Active",
        },
        charges: [],
      },
    ],
  },
];
