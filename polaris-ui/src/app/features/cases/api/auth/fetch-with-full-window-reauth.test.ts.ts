import { CmsAuthError } from "../../../../common/errors/CmsAuthError";
import { CmsAuthRedirectingError } from "../../../../common/errors/CmsAuthRedirectingError";
import { fullWindowReauthenticationFilter } from "./fetch-with-full-window-reauth";
import { generateGuid } from "../generate-guid";

jest.mock("../../../config", () => ({
  REAUTH_REDIRECT_URL_OUTBOUND: "http://foo",
  REAUTH_REDIRECT_URL_OUTBOUND_E2E: "http://bar",
  REAUTH_REDIRECT_URL_INBOUND: "/baz",
}));

describe("Reauthentication Filter", () => {
  const uuid = generateGuid();

  it("can pass through an ok response", () => {
    const response = { ok: true, status: 200 } as Response;
    const filteredResponse = fullWindowReauthenticationFilter(
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
    const filteredResponse = fullWindowReauthenticationFilter(
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
      undefined,
      `http://foo?r=polaris-ui-url%3Dhttp%253A%252F%252Four-ui-domain.com%253FcaseId%253D123%253Fauth-refresh%26fail-correlation-id%3D${uuid}`,
    ],
    [
      "http://our-ui-domain.com", // there is no existing query string
      {},
      `http://bar?r=%2Fbaz%3Fpolaris-ui-url%3Dhttp%253A%252F%252Four-ui-domain.com%253Fauth-refresh%26fail-correlation-id%3D${uuid}`,
    ],
    [
      "http://our-ui-domain.com?caseId=123", // there is an existing query string
      undefined,
      `http://foo?r=%2Fbaz%3Fpolaris-ui-url%3Dhttp%253A%252F%252Four-ui-domain.com%253FcaseId%253D123%2526auth-refresh%26fail-correlation-id%3D${uuid}`,
    ],
    [
      "http://our-ui-domain.com?caseId=123", // there is an existing query string
      {},
      `http://bar?r=%2Fbaz%3Fpolaris-ui-url%3Dhttp%253A%252F%252Four-ui-domain.com%253FcaseId%253D123%2526auth-refresh%26fail-correlation-id%3D${uuid}`,
    ],
  ])(
    "can redirect on a first auth failure ",
    (url, cypressObject, expectedRedirectUrl) => {
      const mockWindow = {
        location: { href: url },
        Cypress: cypressObject,
      } as Window;

      const response = { ok: false, status: 401 } as Response;
      const act = () =>
        fullWindowReauthenticationFilter(response, mockWindow, uuid);

      expect(act).toThrow(CmsAuthRedirectingError);
      expect(mockWindow.location.href).toBe(expectedRedirectUrl);
    }
  );

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

    const act = () =>
      fullWindowReauthenticationFilter(response, mockWindow, uuid);
    expect(act).toThrow(CmsAuthError);
  });

  it.each([
    ["http://our-ui-domain.com?auth-refresh", "http://our-ui-domain.com"],
    [
      // we always append auth-refresh to the end of the querystring, so no need to test if query params come after
      "http://our-ui-domain.com?foo=bar&auth-refresh",
      "http://our-ui-domain.com?foo=bar",
    ],
    [
      "http://our-ui-domain.com?auth-refresh&foo=bar",
      "http://our-ui-domain.com?foo=bar",
    ],
    ["http://our-ui-domain.com?auth-refresh", "http://our-ui-domain.com"],
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

      const filteredResponse = fullWindowReauthenticationFilter(
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
      "It may be the case that you are not yet logged in to CMS, or that CMS has recently logged you out.",
    ],
    [
      "no-cmsauth-cookie",
      "It may be the case that you are not yet logged in to CMS, or that CMS has recently logged you out.",
    ],
    [
      "cms-auth-not-valid",
      "It may be the case that your CMS session has expired.",
    ],
    [
      "cms-modern-auth-not-valid",
      "It may be the case that your CMS session has expired.",
    ],
    [
      "unexpected-error",
      "An unexpected error occurred related to your current CMS log in session.",
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

      const act = () =>
        fullWindowReauthenticationFilter(response, mockWindow, uuid);

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
