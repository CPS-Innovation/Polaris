import faker from "faker";
import { CaseSearchResult } from "../../app/features/cases/domain/gateway/CaseSearchResult";
import { SearchDataSource } from "./types/SearchDataSource";

const dataSource: SearchDataSource = (urn) => {
  const lastDigit = Number(urn?.split("").pop());

  const coreResults = lastDigit ? [...searchResults].slice(-1 * lastDigit) : [];

  return coreResults.map((result) => ({
    ...result,
    uniqueReferenceNumber: urn,
  })) as CaseSearchResult[];
};

export default dataSource;

const searchResults: Omit<CaseSearchResult, "uniqueReferenceNumber">[] = [
  {
    id: 13401,
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
    witnesses: [],
  },
];
