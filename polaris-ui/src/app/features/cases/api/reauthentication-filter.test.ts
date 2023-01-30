import { CmsAuthError } from "../../../common/errors/CmsAuthError";
import { reauthenticationFilter } from "./reauthentication-filter";

jest.mock("../../../config", () => ({
  REAUTH_REDIRECT_URL: "http://foo?q=",
}));

describe("Reauthentication Filter", () => {
  it("can pass through an ok response", () => {
    const response = { ok: true, status: 200 } as Response;
    const filteredResponse = reauthenticationFilter(response, {
      location: { href: "bar" },
    } as Window);

    expect(filteredResponse).toBe(response);
  });

  it("can pass through a non-auth failure response", () => {
    const response = { ok: false, status: 500 } as Response;
    const filteredResponse = reauthenticationFilter(response, {
      location: { href: "bar" },
    } as Window);

    expect(filteredResponse).toBe(response);
  });

  it.each([
    [
      "http://our-ui-domain.com", // there is no existing query string
      "http://foo?q=http%3A%2F%2Four-ui-domain.com%3Fauth-refresh",
    ],
    [
      "http://our-ui-domain.com?caseId=123", // there is an existing query string
      "http://foo?q=http%3A%2F%2Four-ui-domain.com%3FcaseId%3D123%26auth-refresh",
    ],
  ])("can redirect on a first auth failure ", (url, expectedRedirectUrl) => {
    const mockWindow = {
      location: { href: url },
    } as Window;

    const response = { ok: false, status: 403 } as Response;
    const filteredResponse = reauthenticationFilter(response, mockWindow);

    expect(filteredResponse).toBe(response);
    expect(mockWindow.location.href).toBe(expectedRedirectUrl);
  });

  it.each([
    "http://our-ui-domain.com?auth-refresh",
    "http://our-ui-domain.com?foo=bar&auth-refresh",
  ])(
    "can throw if auth fails on a second visit if there is only the reauth token in the query string",
    (url) => {
      const mockWindow = {
        location: { href: url },
      } as Window;

      const response = { ok: false, status: 403 } as Response;

      const act = () => reauthenticationFilter(response, mockWindow);
      expect(act).toThrow(CmsAuthError);
    }
  );

  it.each([
    ["http://our-ui-domain.com?auth-refresh", "http://our-ui-domain.com"],
    [
      "http://our-ui-domain.com?foo=bar&auth-refresh",
      "http://our-ui-domain.com?foo=bar",
    ],
  ])(
    "can clear the redirect flag from the address on a successful call",
    (url, expectedCleanUrl) => {
      const replaceStateMock = jest.fn();

      const mockWindow = {
        location: { href: url },
        history: {
          replaceState: replaceStateMock as typeof window.history.replaceState,
        } as History,
      } as Window;

      const response = { ok: true, status: 200 } as Response;

      const filteredResponse = reauthenticationFilter(response, mockWindow);
      expect(filteredResponse).toBe(response);
      expect(replaceStateMock.mock.calls[0][2]).toBe(expectedCleanUrl);
    }
  );
});
