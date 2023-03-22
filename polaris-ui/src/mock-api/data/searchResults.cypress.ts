import faker from "faker";

import { CaseSearchResult } from "../../app/features/cases/domain/CaseSearchResult";
import { SearchDataSource } from "./types/SearchDataSource";

const dataSource: SearchDataSource = (urn) =>
  searchResults.filter((item) => item.uniqueReferenceNumber.startsWith(urn));

export default dataSource;

const searchResults: CaseSearchResult[] = [
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
      youth: false,
      type: "SOME_TYPE",
    },
    headlineCharge: {
      charge: faker.lorem.sentence(),
      date: "2022-02-01",
      nextHearingDate: "2023-01-02",
    },
  },
  {
    id: 13401,
    uniqueReferenceNumber: "12AB2222222/1",
    isCaseCharged: true,
    numberOfDefendants: 1,
    leadDefendantDetails: {
      id: 901,
      listOrder: 0,
      firstNames: "Steve",
      surname: "Walsh",
      organisationName: "",
      dob: "1977-11-28",
      youth: false,
      type: "SOME_TYPE",
    },
    headlineCharge: {
      charge: faker.lorem.sentence(),
      date: "2022-02-01",
      nextHearingDate: "2023-01-02",
    },
  },
  {
    id: 13401,
    uniqueReferenceNumber: "12AB2222222/2",
    isCaseCharged: true,
    numberOfDefendants: 1,
    leadDefendantDetails: {
      id: 901,
      listOrder: 0,
      firstNames: "Steve",
      surname: "Walsh",
      organisationName: "",
      dob: "1977-11-28",
      youth: false,
      type: "SOME_TYPE",
    },
    headlineCharge: {
      charge: faker.lorem.sentence(),
      date: "2022-02-01",
      nextHearingDate: "2023-01-02",
    },
  },

  {
    id: 13401,
    uniqueReferenceNumber: "12AB2222233",
    isCaseCharged: true,
    numberOfDefendants: 2,
    leadDefendantDetails: {
      id: 901,
      listOrder: 0,
      firstNames: "Steve",
      surname: "Walsh",
      organisationName: "GUZZLERS BREWERY",
      dob: "1977-11-28",
      youth: false,
      type: "Organisation",
    },
    headlineCharge: {
      charge: faker.lorem.sentence(),
      date: "2022-02-01",
      nextHearingDate: "2023-01-02",
    },
  },
];
