import {
  fetchWithFullWindowReauth,
  handleAuthRelatedReload,
} from "./fetch-with-full-window-reauth";
import * as coreModule from "./core";
import * as fetchWithCookiesModule from "./fetch-with-cookies";
import { CORRELATION_ID } from "./core";
import { CmsAuthRedirectingError } from "../../../../common/errors/CmsAuthRedirectingError";
import appInitialisationError from "../../../../app-initialisation-error";
import { CmsAuthError } from "../../../../common/errors/CmsAuthError";

const fetchWithCookiesSpy = jest.spyOn(
  fetchWithCookiesModule,
  "fetchWithCookies"
);

jest.mock("../../../../config", () => ({
  REAUTH_REDIRECT_URL_OUTBOUND: "http://foo,http://foo-2",
  REAUTH_REDIRECT_URL_OUTBOUND_E2E: "http://bar",
  REAUTH_REDIRECT_URL_INBOUND: "/baz",
}));

describe("fetch-with-full-window-reauth", () => {
  afterEach(() => jest.resetAllMocks());

  describe("fetchWithFullWindowReauth", () => {
    it("can return a response for a fetch call that is not an auth error", () => {
      // Arrange
      fetchWithCookiesSpy.mockImplementation(() =>
        Promise.resolve({ status: 403 } as Response)
      );

      // Act
      const response = fetchWithFullWindowReauth("/some-path");

      // Assert
      expect(response).toBeDefined();
    });
  });

  it("can navigate the window when an auth error occurs then also throw to prevent further execution of the app", async () => {
    // Arrange
    fetchWithCookiesSpy.mockImplementation(() =>
      Promise.resolve({ status: 401 } as Response)
    );

    jest
      .spyOn(coreModule, "assembleRedirectUrl")
      .mockImplementationOnce((arg) => "baz");

    const assignSpy = jest.fn();

    Object.defineProperty(window, "location", {
      value: { assign: assignSpy, href: "http://foo" },
    });

    // Act
    const act = async () =>
      await fetchWithFullWindowReauth("/some-path", {
        headers: {
          [CORRELATION_ID]: "foo",
        },
      });

    // Assert
    await expect(act).rejects.toThrowError(CmsAuthRedirectingError);
    expect(assignSpy).toHaveBeenCalledWith("baz");
  });

  describe("handleAuthRelatedReload", () => {
    it("can do nothing if there is no auth-fail-reason param in in window address", () => {
      // Arrange
      const mockWindow = {
        location: {
          search: "",
          href: "http://foo",
        },
      } as Window & typeof globalThis;

      // Act
      const result = handleAuthRelatedReload(mockWindow);

      // Assert
      expect(result).toBe(true);
    });

    it("can strip params from the window address if previously an auth fail but is now ok", () => {
      // Arrange
      const mockWindow = {
        location: {
          search: "?auth-refresh=17&fail-correlation-id=bar&baz=buzz",
          href: "http://foo?auth-refresh=17&fail-correlation-id=bar&baz=buzz",
        },
        history: {
          replaceState: jest.fn() as typeof window.history.replaceState,
        },
      } as Window & typeof globalThis;

      // Act
      const result = handleAuthRelatedReload(mockWindow);

      // Assert
      expect(result).toBe(true);
      expect(mockWindow.history.replaceState).toHaveBeenCalledWith(
        null,
        "",
        "http://foo/?baz=buzz"
      );
    });

    it("can navigate to the next /polaris endpoint and strip params from the window address if reauth failed and there is another outbound endpoint to try", () => {
      // Arrange
      const REFRESH_INDEX = 0;
      const assignSpy = jest.fn();

      const mockWindow = {
        location: {
          search: `?auth-refresh=${REFRESH_INDEX}&fail-correlation-id=bar&auth-fail-reason=cms-auth-not-valid&baz=buzz`,
          href: `http://foo?auth-refresh=${REFRESH_INDEX}&fail-correlation-id=bar&auth-fail-reason=cms-auth-not-valid&baz=buzz`,
          assign: assignSpy as typeof window.location.assign,
        },
      } as Window & typeof globalThis;

      jest
        .spyOn(coreModule, "assembleRedirectUrl")
        .mockImplementationOnce((arg) => "baz");

      expect(appInitialisationError.error).not.toBeDefined();

      // Act
      const act = () => handleAuthRelatedReload(mockWindow);

      // Assert
      expect(act).toThrowError(CmsAuthRedirectingError);
      expect(assignSpy).toHaveBeenCalledWith("baz");
      expect(appInitialisationError.error).not.toBeDefined();
    });

    it("can set the global appInitialisationError and strip params from the window address if reauth failed and there is not another outbound endpoint to try", () => {
      // Arrange
      const REFRESH_INDEX = 1;
      const replaceStateSpy = jest.fn();

      const mockWindow = {
        location: {
          search: `?auth-refresh=${REFRESH_INDEX}&fail-correlation-id=bar&auth-fail-reason=cms-auth-not-valid&baz=buzz`,
          href: `http://foo?auth-refresh=${REFRESH_INDEX}&fail-correlation-id=bar&auth-fail-reason=cms-auth-not-valid&baz=buzz`,
        },
        history: {
          replaceState: replaceStateSpy as typeof window.history.replaceState,
        },
      } as Window & typeof globalThis;

      jest
        .spyOn(coreModule, "assembleRedirectUrl")
        .mockImplementationOnce((arg) => "baz");

      expect(appInitialisationError.error).not.toBeDefined();

      // Act
      handleAuthRelatedReload(mockWindow);

      // Assert
      expect(replaceStateSpy).toHaveBeenCalledWith(
        null,
        "",
        "http://foo/?baz=buzz"
      );
      expect(appInitialisationError.error).toBeDefined();
    });
  });
});
