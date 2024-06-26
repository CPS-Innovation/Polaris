import { PrivateBetaAuthorizationFilter } from "./PrivateBetaAuthorizationFilter";
import * as config from "./../../config";
import { AccountInfo, IPublicClientApplication } from "@azure/msal-browser";
import { render } from "@testing-library/react";

const PRIVATE_BETA_USER_GROUP_VALUE = "foo";
const PRIVATE_BETA_SIGN_UP_URL_VALUE = "http://foo";
const EXISTING_WINDOW_URL = "http://bar";
const EXPECTED_APP_TEXT = "bar";

jest.mock("./../../config", () => ({
  __esModule: true,
}));

// keep typescript happy
const mockConfig = config as {
  PRIVATE_BETA_USER_GROUP: string | null;
  PRIVATE_BETA_SIGN_UP_URL: string;
  PRIVATE_BETA_CHECK_IGNORE_USER: string | null;
};
mockConfig.PRIVATE_BETA_SIGN_UP_URL = PRIVATE_BETA_SIGN_UP_URL_VALUE;

let mockAccounts: AccountInfo[];
const mockMsalInstance = {
  getAllAccounts: () => mockAccounts,
} as IPublicClientApplication;

let mockWindow: Window;

const actFn = () => {
  return render(
    <PrivateBetaAuthorizationFilter
      msalInstance={mockMsalInstance}
      window={mockWindow}
    >
      {EXPECTED_APP_TEXT}
    </PrivateBetaAuthorizationFilter>
  );
};

describe("PrivateBetaAuthorizationFilter", () => {
  beforeEach(() => {
    mockWindow = {
      location: { href: EXISTING_WINDOW_URL },
    } as Window;

    mockAccounts = [];
  });

  it("will allow the user to access the app if no private beta group is configured", () => {
    // Arrange
    mockConfig.PRIVATE_BETA_USER_GROUP = null;

    // Act
    const { findByText } = actFn();

    // Assert
    expect(findByText(EXPECTED_APP_TEXT)).toBeDefined();
    expect(mockWindow.location.href).toBe(EXISTING_WINDOW_URL);
  });

  it("will not allow the user to access the app if the user has no account", () => {
    // Arrange
    mockConfig.PRIVATE_BETA_USER_GROUP = PRIVATE_BETA_USER_GROUP_VALUE;

    // Act
    const { container } = actFn();

    // Assert
    expect(container).toBeEmptyDOMElement();

    expect(mockWindow.location.href).toBe(PRIVATE_BETA_SIGN_UP_URL_VALUE);
  });

  it("will not allow the user to access the app if the user has no groups claims object", () => {
    // Arrange
    mockConfig.PRIVATE_BETA_USER_GROUP = PRIVATE_BETA_USER_GROUP_VALUE;
    mockAccounts = [
      {
        idTokenClaims: {},
      } as AccountInfo,
    ];

    // Act
    const { container } = actFn();

    // Assert
    expect(container).toBeEmptyDOMElement();
    expect(mockWindow.location.href).toBe(PRIVATE_BETA_SIGN_UP_URL_VALUE);
  });

  it("will not allow the user to access the app if the user is not in the private beta group", () => {
    // Arrange
    mockConfig.PRIVATE_BETA_USER_GROUP = PRIVATE_BETA_USER_GROUP_VALUE;
    mockAccounts = [
      {
        idTokenClaims: {
          groups: ["bar", "baz"],
        } as AccountInfo["idTokenClaims"],
      } as AccountInfo,
    ];

    // Act
    const { container } = actFn();

    // Assert
    expect(container).toBeEmptyDOMElement();
    expect(mockWindow.location.href).toBe(PRIVATE_BETA_SIGN_UP_URL_VALUE);
  });

  it("will allow the user to access the app if the user is in the private beta group", () => {
    // Arrange
    mockConfig.PRIVATE_BETA_USER_GROUP = "foo";
    mockAccounts = [
      {
        idTokenClaims: {
          groups: ["bar", "baz", PRIVATE_BETA_USER_GROUP_VALUE],
        } as AccountInfo["idTokenClaims"],
      } as AccountInfo,
    ];

    // Act
    const { findByText } = actFn();

    // Assert
    expect(findByText(EXPECTED_APP_TEXT)).toBeDefined();
    expect(mockWindow.location.href).toBe(EXISTING_WINDOW_URL);
  });

  it.each([["bar@example.org", "bAr@eXaMpLe.orG"]])(
    "will allow the automation user to access the app even if it is not in the private beta group",
    (username) => {
      // Arrange
      mockConfig.PRIVATE_BETA_USER_GROUP = PRIVATE_BETA_USER_GROUP_VALUE;
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = username;
      mockAccounts = [
        {
          idTokenClaims: {
            groups: ["bar", "baz"],
          } as AccountInfo["idTokenClaims"],
          username: "bar@example.org",
        } as AccountInfo,
      ];

      // Act
      const { findByText } = actFn();

      // Assert
      expect(findByText(EXPECTED_APP_TEXT)).toBeDefined();
      expect(mockWindow.location.href).toBe(EXISTING_WINDOW_URL);
    }
  );
});
