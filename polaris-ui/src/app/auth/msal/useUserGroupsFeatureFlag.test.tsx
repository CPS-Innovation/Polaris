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
jest.mock("../../config", () => ({
  __esModule: true,
  PRIVATE_BETA_REDACTION_LOG_USER_GROUP: "abc",
}));

const mockConfig = configModule as {
  PRIVATE_BETA_REDACTION_LOG_USER_GROUP: string;
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
    // empty PRIVATE_BETA_REDACTION_LOG_USER_GROUP
    test("Should return redactionLog feature true, if PRIVATE_BETA_REDACTION_LOG_USER_GROUP is empty and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(true);
    });

    test("Should return redactionLog feature false, if PRIVATE_BETA_REDACTION_LOG_USER_GROUP is empty but FEATURE_FLAG_REDACTION_LOG is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = false;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    // PRIVATE_BETA_REDACTION_LOG_USER_GROUP and REDACTION_LOG_USER_GROUP
    test("Should return redactionLog feature true, if user is in redaction log group and redaction log private beta group", () => {
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
            groups: ["private_beta_redaction_log_group", "redaction_log_group"],
          },
        },
      ]);

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP =
        "private_beta_redaction_log_group";
      mockConfig.REDACTION_LOG_USER_GROUP = "redaction_log_group";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(true);
    });

    test("Should return redactionLog feature false, if user is in redaction log group, but not in private beta redaction log group", () => {
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
            groups: [
              "private_beta_redaction_log_group1",
              "redaction_log_group",
            ],
          },
        },
      ]);

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP =
        "private_beta_redaction_log_group";
      mockConfig.REDACTION_LOG_USER_GROUP = "redaction_log_group";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    test("Should return redactionLog feature false, if user is in private beta redaction log group, but not in redaction log group", () => {
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
            groups: [
              "private_beta_redaction_log_group",
              "redaction_log_group1",
            ],
          },
        },
      ]);

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP =
        "private_beta_redaction_log_group";
      mockConfig.REDACTION_LOG_USER_GROUP = "redaction_log_group";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    test("Should return redactionLog feature false, if user is in redaction log group and redaction log private beta group, but FEATURE_FLAG_REDACTION_LOG is false", () => {
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
            groups: ["private_beta_redaction_log_group", "redaction_log_group"],
          },
        },
      ]);

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = false;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    // cypress integration test and automation test (e2e) user and redactionLog query param
    test("Should return redactionLog feature true, if it is a cypress integration test user, and have redactionLog=true in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ redactionLog: "true" });

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "test";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(true);
    });

    test("Should return redactionLog feature false, if it is a cypress integration test user and does not have redactionLog=true in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "test";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    test("Should return redactionLog feature true, if it is a cypress automation test(e2e) user, and have redactionLog=true in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ redactionLog: "true" });

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(true);
    });

    test("Should return redactionLog feature false, if it is a cypress automation test(e2e) user, and does not have redactionLog=true in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    test("Should return redactionLog feature false, if it is a cypress automation test(e2e) user, and not have redactionLog=true in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });

    test("Should return redactionLog feature false, if it is a cypress integration test user and have redactionLog=true in query param, but FEATURE_FLAG_REDACTION_LOG is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });

      windowSpy.mockImplementation(() => ({ Cypress: {} }));
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ redactionLog: "true" });

      mockConfig.PRIVATE_BETA_REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.REDACTION_LOG_USER_GROUP = "abc";
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "test";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = false;

      const { redactionLog } = useUserGroupsFeatureFlag();
      expect(redactionLog).toStrictEqual(false);
    });
  });
});
