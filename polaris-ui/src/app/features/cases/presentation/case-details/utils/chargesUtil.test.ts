import { getFormattedCustodyTimeData } from "./chargesUtil";
import { ExpiryIndicator } from "../../../domain/CaseDetails";
describe("getFormattedCustodyTimeData util", () => {
  it("Should return correct data when expiryIndicator is expired", () => {
    const custodyTimeLimit = {
      expiryDate: "2022-11-20",
      expiryDays: 20,
      expiryIndicator: ExpiryIndicator.expired,
    };
    const result = getFormattedCustodyTimeData(custodyTimeLimit);
    const expectedResult = {
      custodyExpiryDays: "Expired",
      custodyExpiryDate: "20 Nov 2022",
    };

    expect(result).toEqual(expectedResult);
  });

  it("Should return correct data when expiryIndicator is active", () => {
    const custodyTimeLimit = {
      expiryDate: "2022-11-20",
      expiryDays: 20,
      expiryIndicator: ExpiryIndicator.active,
    };
    const result = getFormattedCustodyTimeData(custodyTimeLimit);
    const expectedResult = {
      custodyExpiryDays: "20 Days",
      custodyExpiryDate: "20 Nov 2022",
    };
    expect(result).toEqual(expectedResult);
  });

  it("Should return correct expiry days string when there is only one day left when expiryIndicator is active", () => {
    const custodyTimeLimit = {
      expiryDate: "2022-11-20",
      expiryDays: 1,
      expiryIndicator: ExpiryIndicator.active,
    };
    const result = getFormattedCustodyTimeData(custodyTimeLimit);
    const expectedResult = {
      custodyExpiryDays: "1 Day",
      custodyExpiryDate: "20 Nov 2022",
    };
    expect(result).toEqual(expectedResult);
  });
  it("Should return correct expiry days string when expiryIndicator is null or not applicable", () => {
    const custodyTimeLimit = {
      expiryDate: "2022-11-20",
      expiryDays: 1,
      expiryIndicator: null,
    };
    const result = getFormattedCustodyTimeData(custodyTimeLimit);
    const expectedResult = {
      custodyExpiryDays: "N/A",
      custodyExpiryDate: "N/A",
    };
    expect(result).toEqual(expectedResult);
  });

  it("Should handle if the custodyTimeLimit is not available at all", () => {
    const custodyTimeLimit = {
      expiryDate: "2022-11-20",
      expiryDays: 1,
      expiryIndicator: null,
    };
    const result = getFormattedCustodyTimeData(undefined as any);
    const expectedResult = {
      custodyExpiryDays: "N/A",
      custodyExpiryDate: "N/A",
    };
    expect(result).toEqual(expectedResult);
  });
});
