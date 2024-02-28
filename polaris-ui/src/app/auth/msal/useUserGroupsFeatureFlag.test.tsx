import { useUserGroupsFeatureFlag } from "./useUserGroupsFeatureFlag";
import * as authModule from "../../auth";
import * as configModule from "../../config";
import * as msalInstanceModule from "./msalInstance";
import * as useQueryParamsStateModule from "../../common/hooks/useQueryParamsState";
jest.mock("./msalInstance", () => ({
  msalInstance: {
    getAllAccounts: jest.fn(),
  },
}));

jest.mock("../../common/hooks/useQueryParamsState", () => ({
  useQueryParamsState: jest.fn(),
}));

jest.mock("../../auth", () => ({
  useUserDetails: jest.fn() as jest.Mock,
}));

const mockConfig = configModule as {
  REDACTION_LOG_USER_GROUP: string;
  FEATURE_FLAG_REDACTION_LOG: boolean;
  PRIVATE_BETA_CHECK_IGNORE_USER: string;
};

describe("useUserGroupsFeatureFlag", () => {
  let windowSpy: any;
  beforeEach(() => {
    windowSpy = jest.spyOn(window, "window", "get");
    (
      useQueryParamsStateModule.useQueryParamsState as jest.Mock
    ).mockReturnValue({});
    jest.resetModules();
    (
      msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
    ).mockReturnValue([
      {
        username: "test_username",
        name: "test_name",
      },
    ]);
  });

  afterEach(() => {
    windowSpy.mockRestore();
  });

  describe("redactionLog feature flag", () => {
    test("Should return redactionLog feature false, if  FEATURE_FLAG_REDACTION_LOG is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          username: "test_username",
          name: "test_name",
          idTokenClaims: {
            groups: [],
          },
        },
      ]);

      mockConfig.FEATURE_FLAG_REDACTION_LOG = false;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    // cypress integration test and automation test (e2e) user and redactionLog query param
    test("Should return redactionLog feature false, if it is a cypress integration test user, and have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ redactionLog: "false" });

      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "test";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    test("Should return redactionLog feature true, if it is a cypress integration test user and does not have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));

      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "test";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(true);
    });

    test("Should return redactionLog feature false, if it is a cypress automation test(e2e) user, and have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ redactionLog: "false" });

      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    test("Should return redactionLog feature true, if it is a cypress automation test(e2e) user, and does not have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));

      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(true);
    });
  });
});
