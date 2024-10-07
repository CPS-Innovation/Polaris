//import fetchMock from "jest-fetch-mock";
import { fetchWithFullWindowReauth } from "./auth/fetch-with-full-window-reauth";
import { fetchWithProactiveInSituReauth } from "./auth/fetch-with-in-situ-reauth";
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
jest.mock("./auth/fetch-with-in-situ-reauth");
jest.mock("./auth/header-factory");
jest.mock("../../../config", () => ({
  GATEWAY_BASE_URL: "https://gateway-url",
}));

const mockOutResponse = (body?: any, init?: ResponseInit | undefined) => {
  var response = new Response(JSON.stringify(body), init);
  (fetchWithFullWindowReauth as jest.Mock).mockReturnValue(response);
  return response;
};

const mockOutResponseProactive = (
  body?: any,
  init?: ResponseInit | undefined
) => {
  var response = new Response(JSON.stringify(body), init);
  (fetchWithProactiveInSituReauth as jest.Mock).mockReturnValue(response);
  return response;
};

describe("gateway-apis", () => {
  beforeEach(() => {
    (HEADERS.correlationId as jest.Mock).mockReturnValue({
      "Correlation-Id": "correlationId_1",
    });
    (HEADERS.auth as jest.Mock).mockReturnValue("test_auth");
  });

  describe("searchUrn", () => {
    it("searchUrn should call the expected flavour of fetch", async () => {
      mockOutResponse(
        { data: "mocked response" },
        {
          status: 200,
        }
      );

      const response = await searchUrn("urn_abc");
      expect(response).toEqual({ data: "mocked response" });
    });

    it("searchUrn should throw error for any other failed response status", async () => {
      mockOutResponse(
        { data: "mocked response" },
        {
          status: 500,
        }
      );

      expect(async () => {
        await searchUrn("urn_abc");
      }).rejects.toThrow();
    });
  });

  describe("getCaseDetails", () => {
    it("getCaseDetails should call the expected flavour of fetch", async () => {
      mockOutResponse(
        { data: "mocked response" },
        {
          status: 200,
        }
      );

      const response = await getCaseDetails("abc", 123);
      expect(response).toEqual({ data: "mocked response" });
    });

    it("getCaseDetails should throw error if for any other failed response status", async () => {
      mockOutResponse(
        { data: "mocked response" },
        {
          status: 500,
          statusText: "OK",
        }
      );

      expect(async () => {
        await getCaseDetails("abc", 122);
      }).rejects.toThrow();
    });
  });

  describe("getPipelinePdfResults", () => {
    it("getPipelinePdfResults should call the expected flavour of fetch", async () => {
      mockOutResponse(
        { documents: [] },
        {
          status: 200,
          statusText: "OK",
        }
      );

      const response = await getPipelinePdfResults("tracker_url", "123");
      expect(response).toEqual({ documents: [] });
    });

    it("getPipelinePdfResults should not throw error if response status is 404 and should return false", async () => {
      mockOutResponse(
        { documents: [] },
        {
          status: 404,
        }
      );
      const response = await getPipelinePdfResults("tracker_url", "123");
      expect(response).toEqual(false);
    });

    it("getPipelinePdfResults should throw error if for any other failed response status ", async () => {
      mockOutResponse(null, {
        status: 500,
      });
      expect(async () => {
        await getPipelinePdfResults("tracker_url", "123");
      }).rejects.toThrow();
    });
  });

  describe("searchCase", () => {
    it("searchCase should call the expected flavour of fetch", async () => {
      mockOutResponse([], {
        status: 200,
      });
      const response = await searchCase("urn_123", 123, "test");

      expect(response).toEqual([]);
    });

    it("searchCase should throw error if for failed response status ", async () => {
      mockOutResponse(
        { documents: [] },
        {
          status: 500,
        }
      );
      expect(async () => {
        await searchCase("urn_123", 123, "test");
      }).rejects.toThrow();
    });
  });

  describe("checkoutDocument", () => {
    it("checkoutDocument should call the expected flavour of fetch", async () => {
      mockOutResponse("success", {
        status: 200,
      });
      const response = await checkoutDocument("urn_123", 123, "documentID_1");

      expect(response).toEqual(true);
    });

    it("checkoutDocument should throw error if for failed response status ", async () => {
      mockOutResponse("success", {
        status: 500,
      });
      expect(async () => {
        await checkoutDocument("urn_123", 123, "documentID_1");
      }).rejects.toThrow();
    });
  });

  describe("cancelCheckoutDocument", () => {
    it("cancelCheckoutDocument should call the expected flavour of fetch", async () => {
      mockOutResponse("success", {
        status: 200,
      });
      const response = await cancelCheckoutDocument(
        "urn_123",
        123,
        "documentID_1"
      );

      expect(response).toEqual(true);
    });

    it("cancelCheckoutDocument should throw error if for failed response status ", async () => {
      mockOutResponse("Internal server Error", {
        status: 500,
      });
      expect(async () => {
        await cancelCheckoutDocument("urn_123", 123, "documentID_1");
      }).rejects.toThrow();
    });
  });

  describe("saveRedactions", () => {
    it("saveRedactions should call the expected flavour of fetch", async () => {
      mockOutResponse("success", {
        status: 200,
      });

      const responseFlag = await (async () => {
        await saveRedactions("urn_123", 123, "documentID_1", {
          documentId: "documentID_1",
          redactions: [],
          documentModifications: [],
        });
        return true;
      })();

      expect(responseFlag).toBe(true);
    });

    it("saveRedactions should throw error if for failed response status ", async () => {
      mockOutResponse("Internal server Error", {
        status: 500,
      });

      expect(async () => {
        await saveRedactions("urn_123", 123, "documentID_1", {
          documentId: "documentID_1",
          redactions: [],
          documentModifications: [],
        });
      }).rejects.toThrow();
    });
  });

  describe("initiatePipeline", () => {
    beforeEach(() => {
      fetchMock.resetMocks();
    });

    it("initiatePipeline should call fetch and should not call reauthentication", async () => {
      mockOutResponse(
        { trackerUrl: "https://tracker_url/" },
        {
          status: 200,
        }
      );
      const response = await initiatePipeline("abc", 123, "correlationId_1");

      expect(response).toEqual({
        correlationId: "correlationId_1",
        status: 200,
        trackerUrl: "https://tracker_url/",
      });
    });

    it("initiatePipeline should not throw error if response status is 423", async () => {
      mockOutResponse(
        { trackerUrl: "https://tracker_url/" },
        {
          status: 423,
        }
      );
      const response = await initiatePipeline("abc", 123, "correlationId_1");

      expect(response).toEqual({
        correlationId: "correlationId_1",
        status: 423,
        trackerUrl: "https://tracker_url/",
      });
    });

    it("initiatePipeline should resolve a relative tracker url to a fully-qualified url", async () => {
      mockOutResponse(
        { trackerUrl: "tracker_url" },
        {
          status: 200,
        }
      );
      const response = await initiatePipeline("abc", 123, "correlationId_1");
      expect(response.trackerUrl).toEqual("https://gateway-url/tracker_url");
    });

    it("initiatePipeline should throw error if for any other failed response status", async () => {
      mockOutResponse(
        { trackerUrl: "tracker_url" },
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
