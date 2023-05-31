import { searchUrn, getCaseDetails, getPdfSasUrl } from "./gateway-api";
import fetchMock from "jest-fetch-mock";
import { reauthenticationFilter } from "./reauthentication-filter";
jest.mock("./reauthentication-filter");
describe.only("gateway-apis", () => {
  describe("searchUrn", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });
    it("searchUrn should call fetch and reauthenticationFilter", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 200,
          statusText: "OK",
          headers: { "Content-type": "application/json" },
        }
      );

      (reauthenticationFilter as jest.Mock).mockReturnValue(mockResponse);
      fetchMock.mockResponseOnce(JSON.stringify({ data: "mocked response" }));
      const response = await searchUrn("abc");
      expect(reauthenticationFilter).toHaveBeenCalledTimes(1);
      expect(response).toEqual({ data: "mocked response" });
    });
    it("searchUrn should not throw error if response status is 404", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 404,
          statusText: "OK",
          headers: { "Content-type": "application/json" },
        }
      );

      (reauthenticationFilter as jest.Mock).mockReturnValue(mockResponse);
      fetchMock.mockResponseOnce(JSON.stringify({ data: "mocked response" }));
      const response = await searchUrn("abc");
      expect(reauthenticationFilter).toHaveBeenCalledTimes(1);
      expect(response).toEqual([]);
    });
    it("searchUrn should throw error if for any other failed response status ", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 500,
          statusText: "OK",
          headers: { "Content-type": "application/json" },
        }
      );
      fetchMock.mockResponseOnce(JSON.stringify({ data: "mocked response" }));
      (reauthenticationFilter as jest.Mock).mockReturnValue(mockResponse);

      expect(async () => {
        await searchUrn("abc");
      }).rejects.toThrow(
        "An error ocurred contacting the server at https://polaris-dev-cmsproxy.azurewebsites.net/api/urns/abc/cases: Search URN failed; status - OK (500)"
      );
      expect(reauthenticationFilter).toHaveBeenCalledTimes(0);
    });
  });

  describe("getCaseDetails", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });
    it("getCaseDetails should call fetch and reauthenticationFilter", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 200,
          statusText: "OK",
          headers: { "Content-type": "application/json" },
        }
      );

      (reauthenticationFilter as jest.Mock).mockReturnValue(mockResponse);
      fetchMock.mockResponseOnce(JSON.stringify({ data: "mocked response" }));

      const response = await getCaseDetails("abc", 123);
      expect(reauthenticationFilter).toHaveBeenCalledTimes(1);
      expect(response).toEqual({ data: "mocked response" });
    });

    it("getCaseDetails should throw error if for any other failed response status ", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 500,
          statusText: "OK",
          headers: { "Content-type": "application/json" },
        }
      );
      fetchMock.mockResponseOnce(JSON.stringify({ data: "mocked response" }));
      (reauthenticationFilter as jest.Mock).mockReturnValue(mockResponse);

      expect(async () => {
        await getCaseDetails("abc", 122);
      }).rejects.toThrow(
        "An error ocurred contacting the server at https://polaris-dev-cmsproxy.azurewebsites.net/api/urns/abc/cases/122: Get Case Details failed; status - OK (500)"
      );
      expect(reauthenticationFilter).toHaveBeenCalledTimes(0);
    });
  });

  describe("getPdfSasUrl", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });
    it("getPdfSasUrl should call fetch and reauthenticationFilter", async () => {
      fetchMock.mockResponseOnce("mocked response");
      const response = await getPdfSasUrl("abc", 123, "ABC");
      expect(reauthenticationFilter).toHaveBeenCalledTimes(0);
      expect(response).toEqual("mocked response");
    });

    it("getPdfSasUrl should throw error if for any other failed response status ", async () => {
      fetchMock.mockResponseOnce("Internal Server Error", { status: 500 });

      expect(async () => {
        await getPdfSasUrl("abc", 123, "ABC");
      }).rejects.toThrow(
        "An error ocurred contacting the server at https://polaris-dev-cmsproxy.azurewebsites.net/api/urns/abc/cases/123/documents/ABC/sasUrl: Get Pdf SasUrl failed; status - Internal Server Error (500)"
      );
      expect(reauthenticationFilter).toHaveBeenCalledTimes(0);
    });
  });
});
