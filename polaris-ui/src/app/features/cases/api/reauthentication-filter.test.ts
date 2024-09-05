import { CmsAuthError } from "../../../common/errors/CmsAuthError";
import { CmsAuthRedirectingError } from "../../../common/errors/CmsAuthRedirectingError";
import { reauthenticationFilter } from "./reauthentication-filter";
import { generateGuid } from "./generate-guid";

jest.mock("../../../config", () => ({
  REAUTH_REDIRECT_URL: "http://foo?r=polaris-ui-url%3D",
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
      `http://foo?r=polaris-ui-url%3Dhttp%253A%252F%252Four-ui-domain.com%253Fauth-refresh%26fail-correlation-id%3D${uuid}`,
    ],
    [
      "http://our-ui-domain.com?caseId=123", // there is an existing query string
      `http://foo?r=polaris-ui-url%3Dhttp%253A%252F%252Four-ui-domain.com%253FcaseId%253D123%2526auth-refresh%26fail-correlation-id%3D${uuid}`,
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
  it.each([
    [
      "no-cookies",
      "You are not logged in to CMS, please try logging in to CMS again.",
    ],
    [
      "no-cmsauth-cookie",
      "You are not logged in to CMS, please try logging in to CMS again.",
    ],
    [
      "cms-auth-not-valid",
      "Your CMS session has expired, please try logging in to CMS again.",
    ],
    [
      "cms-modern-auth-not-valid",
      "Your CMS session has expired, please try logging in to CMS again.",
    ],
    [
      "unexpected-error",
      "An unexpected error occurred, please try logging in to CMS again.",
    ],
  ])(
    "throws CmsAuthError with correct message for auth-fail-reason=%s",
    (authFailReason, expectedMessage) => {
      const mockWindow = {
        location: {
          href: `http://our-ui-domain.com?auth-refresh&auth-fail-reason=${authFailReason}`,
          search: `?auth-refresh&auth-fail-reason=${authFailReason}`,
        },
      } as Window;

      const response = { ok: false, status: 401 } as Response;

      const act = () => reauthenticationFilter(response, mockWindow, uuid);

      expect(act).toThrow(CmsAuthError);
      try {
        act();
      } catch (e) {
        if (e instanceof CmsAuthError) {
          expect(e.customMessage).toBe(expectedMessage);
        }
      }
    }
  );
});
