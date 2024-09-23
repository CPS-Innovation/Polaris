import fetchMock from "jest-fetch-mock";
import { fetchWithFullWindowReauth } from "./auth/fetch-with-full-window-reauth";
import * as HEADERS from "./auth/header-factory";
import {
  searchUrn,
  getCaseDetails,
  initiatePipeline,
  getPipelinePdfResults,
  searchCase,
  checkoutDocument,
  cancelCheckoutDocument,
  saveRedactions,
} from "./gateway-api";

jest.mock("./auth/fetch-with-full-window-reauth");
jest.mock("./auth/header-factory");
jest.mock("../../../config", () => ({
  GATEWAY_BASE_URL: "https://gateway-url",
}));
describe("gateway-apis", () => {
  beforeEach(() => {
    (HEADERS.correlationId as jest.Mock).mockReturnValue({
      "Correlation-Id": "correlationId_1",
    });
    (HEADERS.auth as jest.Mock).mockReturnValue("test_auth");
  });

  describe("searchUrn", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("searchUrn should call fetch and reauthentication", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 200,
          statusText: "OK",
        }
      );

      (fetchWithFullWindowReauth as jest.Mock).mockReturnValue(mockResponse);

      const response = await searchUrn("urn_abc");
      expect(response).toEqual({ data: "mocked response" });
    });

    it("searchUrn should throw error if for any other failed response status", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 500,
          statusText: "OK",
          headers: { "Content-type": "application/json" },
        }
      );
      fetchMock.mockResponseOnce(JSON.stringify(mockResponse));
      (fetchWithFullWindowReauth as jest.Mock).mockReturnValue(mockResponse);

      expect(async () => {
        await searchUrn("urn_abc");
      }).rejects.toThrow(
        "An error occurred contacting the server at https://gateway-url/api/urns/urn_abc/cases: Search URN failed; status - OK (500)"
      );
    });
  });

  describe("getCaseDetails", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("getCaseDetails should call fetch and call reauthentication", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 200,
          statusText: "OK",
        }
      );

      (fetchWithFullWindowReauth as jest.Mock).mockReturnValue(mockResponse);
      fetchMock.mockResponseOnce(JSON.stringify({ data: "mocked response" }));

      const response = await getCaseDetails("abc", 123);
      expect(response).toEqual({ data: "mocked response" });
    });

    it("getCaseDetails should throw error if for any other failed response status", async () => {
      const mockResponse = new Response(
        JSON.stringify({ data: "mocked response" }),
        {
          status: 500,
          statusText: "OK",
        }
      );
      fetchMock.mockResponseOnce(JSON.stringify(mockResponse));
      (fetchWithFullWindowReauth as jest.Mock).mockReturnValue(mockResponse);

      expect(async () => {
        await getCaseDetails("abc", 122);
      }).rejects.toThrow(
        "An error occurred contacting the server at https://gateway-url/api/urns/abc/cases/122: Get Case Details failed; status - OK (500)"
      );
    });
  });

  describe("getPipelinePdfResults", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("getPipelinePdfResults should call fetch and should not call reauthentication", async () => {
      fetchMock.mockResponseOnce(JSON.stringify({ documents: [] }), {
        status: 200,
      });
      const response = await getPipelinePdfResults("tracker_url", "123");
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledWith("tracker_url", expect.anything());
      expect(response).toEqual({ documents: [] });
    });

    it("getPipelinePdfResults should not throw error if response status is 404 and should return false", async () => {
      fetchMock.mockResponseOnce(JSON.stringify({ documents: [] }), {
        status: 404,
      });
      const response = await getPipelinePdfResults("tracker_url", "123");
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledWith("tracker_url", expect.anything());
      expect(response).toEqual(false);
    });

    it("getPipelinePdfResults should throw error if for any other failed response status ", async () => {
      fetchMock.mockResponseOnce(JSON.stringify({ documents: [] }), {
        status: 500,
      });
      expect(async () => {
        await getPipelinePdfResults("tracker_url", "123");
      }).rejects.toThrow(
        "An error occurred contacting the server at tracker_url: Get Pipeline pdf results failed; status - Internal Server Error (500)"
      );
    });
  });

  describe("searchCase", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("searchCase should call fetch and should not call reauthentication", async () => {
      fetchMock.mockResponseOnce(JSON.stringify([]), {
        status: 200,
      });
      const response = await searchCase("urn_123", 123, "test");
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledWith(
        "https://gateway-url/api/urns/urn_123/cases/123/search/?query=test",
        expect.anything()
      );
      expect(response).toEqual([]);
    });

    it("searchCase should throw error if for failed response status ", async () => {
      fetchMock.mockResponseOnce(JSON.stringify({ documents: [] }), {
        status: 500,
      });
      expect(async () => {
        await searchCase("urn_123", 123, "test");
      }).rejects.toThrow(
        "An error occurred contacting the server at https://gateway-url/api/urns/urn_123/cases/123/search/?query=test: Search Case Text failed; status - Internal Server Error (500)"
      );
    });
  });

  describe("checkoutDocument", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("checkoutDocument should call fetch and should not call reauthentication", async () => {
      fetchMock.mockResponseOnce(JSON.stringify("success"), {
        status: 200,
      });
      const response = await checkoutDocument("urn_123", 123, "documentID_1");
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledWith(
        "https://gateway-url/api/urns/urn_123/cases/123/documents/documentID_1/checkout",
        expect.anything()
      );
      expect(response).toEqual(true);
    });

    it("checkoutDocument should throw error if for failed response status ", async () => {
      fetchMock.mockResponseOnce(JSON.stringify("Internal server Error"), {
        status: 500,
      });
      expect(async () => {
        await checkoutDocument("urn_123", 123, "documentID_1");
      }).rejects.toThrow(
        "An error occurred contacting the server at https://gateway-url/api/urns/urn_123/cases/123/documents/documentID_1/checkout: Checkout document failed; status - Internal Server Error (500)"
      );
    });
  });

  describe("cancelCheckoutDocument", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("cancelCheckoutDocument should call fetch and should not call reauthentication", async () => {
      fetchMock.mockResponseOnce(JSON.stringify("success"), {
        status: 200,
      });
      const response = await cancelCheckoutDocument(
        "urn_123",
        123,
        "documentID_1"
      );
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledWith(
        "https://gateway-url/api/urns/urn_123/cases/123/documents/documentID_1/checkout",
        expect.anything()
      );
      expect(response).toEqual(true);
    });

    it("cancelCheckoutDocument should throw error if for failed response status ", async () => {
      fetchMock.mockResponseOnce(JSON.stringify("Internal server Error"), {
        status: 500,
      });
      expect(async () => {
        await cancelCheckoutDocument("urn_123", 123, "documentID_1");
      }).rejects.toThrow(
        "An error occurred contacting the server at https://gateway-url/api/urns/urn_123/cases/123/documents/documentID_1/checkout: Checkin document failed; status - Internal Server Error (500)"
      );
    });
  });

  describe("saveRedactions", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("saveRedactions should call fetch and should not call reauthentication", async () => {
      fetchMock.mockResponseOnce(JSON.stringify("success"), {
        status: 200,
      });
      await saveRedactions("urn_123", 123, "documentID_1", {
        documentId: "documentID_1",
        redactions: [],
      });
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledWith(
        "https://gateway-url/api/urns/urn_123/cases/123/documents/documentID_1",
        expect.anything()
      );
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
    });

    it("saveRedactions should throw error if for failed response status ", async () => {
      fetchMock.mockResponseOnce(JSON.stringify("Internal server Error"), {
        status: 500,
      });

      expect(async () => {
        await saveRedactions("urn_123", 123, "documentID_1", {
          documentId: "documentID_1",
          redactions: [],
        });
      }).rejects.toThrow(
        "An error occurred contacting the server at https://gateway-url/api/urns/urn_123/cases/123/documents/documentID_1: Save redactions failed; status - Internal Server Error (500)"
      );
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
    });
  });

  describe("initiatePipeline", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("initiatePipeline should call fetch and should not call reauthentication", async () => {
      fetchMock.mockResponseOnce(
        JSON.stringify({ trackerUrl: "https://tracker_url/" }),
        {
          status: 200,
        }
      );
      const response = await initiatePipeline("abc", 123, "correlationId_1");
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
      expect(fetchMock).toHaveBeenCalledWith(
        "https://gateway-url/api/urns/abc/cases/123",
        expect.anything()
      );
      expect(response).toEqual({
        correlationId: "correlationId_1",
        status: 200,
        trackerUrl: "https://tracker_url/",
      });
    });

    it("initiatePipeline should not throw error if response status is 423", async () => {
      fetchMock.mockResponseOnce(
        JSON.stringify({ trackerUrl: "https://tracker_url/" }),
        {
          status: 423,
        }
      );
      const response = await initiatePipeline("abc", 123, "correlationId_1");
      expect(fetchWithFullWindowReauth).toHaveBeenCalledTimes(0);
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock).toHaveBeenCalledWith(
        "https://gateway-url/api/urns/abc/cases/123",
        expect.anything()
      );
      expect(response).toEqual({
        correlationId: "correlationId_1",
        status: 423,
        trackerUrl: "https://tracker_url/",
      });
    });

    it("initiatePipeline should resolve a relative tracker url to a fully-qualified url", async () => {
      fetchMock.mockResponseOnce(
        JSON.stringify({ trackerUrl: "tracker_url" }),
        {
          status: 200,
        }
      );
      const response = await initiatePipeline("abc", 123, "correlationId_1");
      expect(response.trackerUrl).toEqual("https://gateway-url/tracker_url");
    });

    it("initiatePipeline should throw error if for any other failed response status", async () => {
      fetchMock.mockResponseOnce(
        JSON.stringify({ trackerUrl: "tracker_url" }),
        {
          status: 500,
        }
      );
      expect(async () => {
        await initiatePipeline("abc", 123, "correlationId_1");
      }).rejects.toThrow(
        "An error occurred contacting the server at https://gateway-url/api/urns/abc/cases/123: Initiate pipeline failed; status - Internal Server Error (500)"
      );
    });
  });
});
