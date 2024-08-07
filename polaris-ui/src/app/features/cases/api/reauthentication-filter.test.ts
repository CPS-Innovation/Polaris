import { CmsAuthError } from "../../../common/errors/CmsAuthError";
import { CmsAuthRedirectingError } from "../../../common/errors/CmsAuthRedirectingError";
import { reauthenticationFilter } from "./reauthentication-filter";
import { generateGuid } from "./generate-guid";

jest.mock("../../../config", () => ({
  REAUTH_REDIRECT_URL: "http://foo?polaris-ui-url=",
}));

describe("Reauthentication Filter", () => {
  const uuid = generateGuid();

  it("can pass through an ok response", () => {
    const response = { ok: true, status: 200 } as Response;
    const filteredResponse = reauthenticationFilter(
      response,
      {
        location: { href: "bar" },
      } as Window,
      uuid
    );

    expect(filteredResponse).toBe(response);
  });

  it("can pass through a non-auth failure response", () => {
    const response = { ok: false, status: 500 } as Response;
    const filteredResponse = reauthenticationFilter(
      response,
      {
        location: { href: "bar" },
      } as Window,
      uuid
    );

    expect(filteredResponse).toBe(response);
  });

  it.each([
    [
      "http://our-ui-domain.com", // there is no existing query string
      `http://foo?polaris-ui-url=http%3A%2F%2Four-ui-domain.com%3Fauth-refresh%26fail-correlation-id%3D${uuid}`,
    ],
    [
      "http://our-ui-domain.com?caseId=123", // there is an existing query string
      `http://foo?polaris-ui-url=http%3A%2F%2Four-ui-domain.com%3FcaseId%3D123%26auth-refresh%26fail-correlation-id%3D${uuid}`,
    ],
  ])("can redirect on a first auth failure ", (url, expectedRedirectUrl) => {
    const mockWindow = {
      location: { href: url },
    } as Window;

    const response = { ok: false, status: 401 } as Response;
    const act = () => reauthenticationFilter(response, mockWindow, uuid);

    expect(act).toThrow(CmsAuthRedirectingError);
    expect(mockWindow.location.href).toBe(expectedRedirectUrl);
  });

  it.each([
    ["http://our-ui-domain.com?auth-refresh", "http://our-ui-domain.com"],
    [
      "http://our-ui-domain.com?foo=bar&auth-refresh",
      "http://our-ui-domain.com?foo=bar",
    ],
  ])("can throw if auth fails on a second visit", (url, expectedCleanUrl) => {
    const replaceStateMock = jest.fn();

    const mockWindow = {
      location: { href: url },
      history: {
        replaceState: replaceStateMock as typeof window.history.replaceState,
      } as History,
    } as Window;

    const response = { ok: false, status: 401 } as Response;

    const act = () => reauthenticationFilter(response, mockWindow, uuid);
    expect(act).toThrow(CmsAuthError);
    expect(replaceStateMock.mock.calls[0][2]).toBe(expectedCleanUrl);
  });

  it.each([
    ["http://our-ui-domain.com?auth-refresh", "http://our-ui-domain.com"],
    [
      // we always append auth-refresh to the end of the querystring, so no need to test if query params come after
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

      const filteredResponse = reauthenticationFilter(
        response,
        mockWindow,
        uuid
      );
      expect(filteredResponse).toBe(response);
      expect(replaceStateMock.mock.calls[0][2]).toBe(expectedCleanUrl);
    }
  );
});
