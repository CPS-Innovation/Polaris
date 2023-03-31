import { isMultipleChargeCase } from "./isMultipleChargeCase";
import { CaseDetails } from "../../../domain/gateway/CaseDetails";
describe("isMultipleChargeCase util", () => {
  it("Should return true if there are more than one defendants in caseDetails", () => {
    const caseDetails: CaseDetails = {
      id: 13401,
      defendants: [
        {
          defendantDetails: {},
          custodyTimeLimit: {},
          charges: [],
        },
        {
          defendantDetails: {},
          custodyTimeLimit: {},
          charges: [],
        },
      ],
    } as any;
    const result = isMultipleChargeCase(caseDetails);
    expect(result).toEqual(true);
  });

  it("Should return true if there is only one defendant and multiple charges in caseDetails", () => {
    const caseDetails: CaseDetails = {
      id: 13401,
      defendants: [
        {
          defendantDetails: {},
          custodyTimeLimit: {},
          charges: [{}, {}],
        },
      ],
    } as any;
    const result = isMultipleChargeCase(caseDetails);
    expect(result).toEqual(true);
  });

  it("Should return false if there is only one defendant and less than two charges, in caseDetails", () => {
    const caseDetails: CaseDetails = {
      id: 13401,
      defendants: [
        {
          defendantDetails: {},
          custodyTimeLimit: {},
          charges: [{}],
        },
      ],
    } as any;
    const result = isMultipleChargeCase(caseDetails);
    expect(result).toEqual(false);
  });

  it("Should not break if the defendants charges array is empty", () => {
    const caseDetails: CaseDetails = {
      id: 13401,
      defendants: [
        {
          defendantDetails: {},
          custodyTimeLimit: {},
          charges: [],
        },
      ],
    } as any;
    const result = isMultipleChargeCase(caseDetails);
    expect(result).toEqual(false);
  });

  it("Should not break if the caseDetails.defendants array is empty", () => {
    const caseDetails: CaseDetails = {
      id: 13401,
      defendants: [],
    } as any;
    const result = isMultipleChargeCase(caseDetails);
    expect(result).toEqual(false);
  });
});
