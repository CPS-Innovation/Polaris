import * as appInsightsUtils from "./appInsightsUtils";
import fetchMock from "jest-fetch-mock";

describe("AppInsightsUitls", () => {
  describe("testAppInsightsConnection", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    test("It should return false if the appInsights object is not available", async () => {
      jest.spyOn(appInsightsUtils, "getAppInsights").mockReturnValue(null);
      const result = await appInsightsUtils.testAppInsightsConnection();
      expect(appInsightsUtils.getAppInsights).toHaveBeenCalledTimes(1);
      expect(result).toEqual(false);
    });

    test("It should return false if the instrumentationKey or endpointUrl is not available in the appInsights config object", async () => {
      jest
        .spyOn(appInsightsUtils, "getAppInsights")
        .mockReturnValue({ config: {} } as any);
      const result = await appInsightsUtils.testAppInsightsConnection();
      expect(appInsightsUtils.getAppInsights).toHaveBeenCalledTimes(1);
      expect(result).toEqual(false);
    });

    test("It should return false if the post request response object errors array is non empty", async () => {
      const consoleErrorSpy = jest
        .spyOn(console, "error")
        .mockImplementation(() => {});
      fetchMock.mockResponseOnce(JSON.stringify({ errors: [{}] }));
      jest.spyOn(appInsightsUtils, "getAppInsights").mockReturnValue({
        config: {
          instrumentationKey: "abc",
          endpointUrl: "test_url",
        },
      } as any);
      const result = await appInsightsUtils.testAppInsightsConnection();
      expect(appInsightsUtils.getAppInsights).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(consoleErrorSpy).toHaveBeenCalledWith("AppInsight Error", [{}]);
      expect(result).toEqual(false);
      consoleErrorSpy.mockRestore();
    });

    test("It should return false if the post request has failed to due to some other error", async () => {
      const consoleErrorSpy = jest
        .spyOn(console, "error")
        .mockImplementation(() => {});
      fetchMock.mockResponseOnce("Internal Server Error", { status: 500 });
      jest.spyOn(appInsightsUtils, "getAppInsights").mockReturnValue({
        config: {
          instrumentationKey: "abc",
          endpointUrl: "test_url",
        },
      } as any);
      const result = await appInsightsUtils.testAppInsightsConnection();
      expect(appInsightsUtils.getAppInsights).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        "AppInsight request error",
        expect.anything()
      );
      expect(result).toEqual(false);
      consoleErrorSpy.mockRestore();
    });

    test("It should return true if the post request was success and response  object errors array is empty", async () => {
      const consoleErrorSpy = jest
        .spyOn(console, "error")
        .mockImplementation(() => {});
      fetchMock.mockResponseOnce(JSON.stringify({ errors: [] }));
      jest.spyOn(appInsightsUtils, "getAppInsights").mockReturnValue({
        config: {
          instrumentationKey: "abc",
          endpointUrl: "test_url",
        },
      } as any);
      const result = await appInsightsUtils.testAppInsightsConnection();
      expect(appInsightsUtils.getAppInsights).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(consoleErrorSpy).toHaveBeenCalledTimes(0);
      expect(result).toEqual(true);
      consoleErrorSpy.mockRestore();
    });
  });
});
