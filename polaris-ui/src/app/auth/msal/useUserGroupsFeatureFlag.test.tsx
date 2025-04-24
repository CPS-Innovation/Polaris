import { renderHook, act } from "@testing-library/react";
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
  FEATURE_FLAG_REDACTION_LOG: boolean;
  PRIVATE_BETA_CHECK_IGNORE_USER: string;
  FEATURE_FLAG_NOTES: boolean;
  FEATURE_FLAG_SEARCH_PII: boolean;
  FEATURE_FLAG_RENAME_DOCUMENT: boolean;
  FEATURE_FLAG_RECLASSIFY: boolean;
  FEATURE_FLAG_PAGE_DELETE: boolean;
  FEATURE_FLAG_PAGE_ROTATE: boolean;
  FEATURE_FLAG_STATE_RETENTION: boolean;
  FEATURE_FLAG_GLOBAL_NAV: boolean;
  FEATURE_FLAG_REDACTION_TOGGLE_COPY_BUTTON: boolean;
  FEATURE_FLAG_DOCUMENT_NAME_SEARCH: boolean;
  PRIVATE_BETA_FEATURE_USER_GROUP: string;
  PRIVATE_BETA_FEATURE_USER_GROUP2: string;
  PRIVATE_BETA_FEATURE_USER_GROUP5: string;
};

describe("useUserGroupsFeatureFlag", () => {
  beforeEach(() => {
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
    window.Cypress = undefined;
  });

  describe("redactionLog feature flag", () => {
    test("Should return redactionLog feature false, if  FEATURE_FLAG_REDACTION_LOG is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      mockConfig.FEATURE_FLAG_REDACTION_LOG = false;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.redactionLog).toStrictEqual(false);
    });
    // cypress integration test and automation test (e2e) user and redactionLog query param
    test("Should return redactionLog feature false, if it is a cypress integration test user, and have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      window.Cypress = {};
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ redactionLog: "false" });
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "test";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.redactionLog).toStrictEqual(false);
    });
    test("Should return redactionLog feature true, if it is a cypress integration test user and does not have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      window.Cypress = {};
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "test";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.redactionLog).toStrictEqual(true);
    });
    test("Should return redactionLog feature false, if it is a cypress automation test(e2e) user, and have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      window.Cypress = {};
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ redactionLog: "false" });
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.redactionLog).toStrictEqual(false);
    });
    test("Should return redactionLog feature true, if it is a cypress automation test(e2e) user, and does not have redactionLog=false in query param and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      window.Cypress = {};
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.redactionLog).toStrictEqual(true);
    });
    test("Should return redactionLog feature true, if user is not in private beta feature groups and FEATURE_FLAG_REDACTION_LOG is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group1"],
          },
        },
      ]);
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      mockConfig.FEATURE_FLAG_REDACTION_LOG = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.redactionLog).toStrictEqual(true);
    });
  });

  describe("redaction copy button flag", () => {
    test("Should show the button if REACT_APP_FEATURE_FLAG_REDACTION_TOGGLE_COPY_BUTTON is set to true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      mockConfig.FEATURE_FLAG_REDACTION_TOGGLE_COPY_BUTTON = true;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.copyRedactionTextButton).toStrictEqual(true);
    });
  });

  describe("document name search flag", () => {
    test("Should return documentNameSearch feature false, if FEATURE_FLAG_DOCUMENT_NAME_SEARCH is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      mockConfig.FEATURE_FLAG_DOCUMENT_NAME_SEARCH = false;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.documentNameSearch).toStrictEqual(false);
    });

    test("Should return documentNameSearch feature true, if user is not in private beta feature groups and FEATURE_FLAG_DOCUMENT_NAME_SEARCH is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group5"],
          },
        },
      ]);
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP5 =
        "private_beta_feature_group5";
      mockConfig.FEATURE_FLAG_DOCUMENT_NAME_SEARCH = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.documentNameSearch).toStrictEqual(true);
    });
  });

  describe("notes feature flag", () => {
    test("Should return notes feature false, if  FEATURE_FLAG_NOTES is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);

      mockConfig.FEATURE_FLAG_NOTES = false;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.notes).toStrictEqual(false);
    });

    test("Should return notes feature true, if it is a cypress integration test user and does not have notes=false in query param and FEATURE_FLAG_NOTES is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      mockConfig.FEATURE_FLAG_NOTES = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.notes).toStrictEqual(true);
    });

    test("Should return notes feature true, if it is a cypress automation test(e2e) user, and does not have notes=false in query param and FEATURE_FLAG_NOTES is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_NOTES = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.notes).toStrictEqual(true);
    });

    test("Should return notes feature false, if it is a cypress automation test(e2e) user, and have notes=false in query param and FEATURE_FLAG_NOTES is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ notes: "false" });
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_NOTES = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.notes).toStrictEqual(false);
    });

    test("Should return notes feature true, if user is not in private beta feature groups and FEATURE_FLAG_NOTES is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group1"],
          },
        },
      ]);
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      mockConfig.FEATURE_FLAG_NOTES = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.notes).toStrictEqual(true);
    });
  });

  describe("searchPII feature flag", () => {
    test("Should return searchPII feature false, if FEATURE_FLAG_SEARCH_PII is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);

      mockConfig.FEATURE_FLAG_SEARCH_PII = false;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.searchPII).toStrictEqual(false);
    });

    test("Should return searchPII feature true, if it is a cypress integration test user and does not have notes=false in query param and FEATURE_FLAG_SEARCH_PII is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      mockConfig.FEATURE_FLAG_SEARCH_PII = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.searchPII).toStrictEqual(true);
    });

    test("Should return searchPII feature true, if it is a cypress automation test(e2e) user, and does not have notes=false in query param and FEATURE_FLAG_SEARCH_PII is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_SEARCH_PII = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.searchPII).toStrictEqual(true);
    });

    test("Should return searchPII feature false, if it is a cypress automation test(e2e) user, and have notes=false in query param and FEATURE_FLAG_SEARCH_PII is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ searchPII: "false" });
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_SEARCH_PII = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.searchPII).toStrictEqual(false);
    });

    test("Should return searchPII feature false, if user is not in private beta feature group and FEATURE_FLAG_SEARCH_PII is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group1"],
          },
        },
      ]);
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";
      mockConfig.FEATURE_FLAG_SEARCH_PII = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.searchPII).toStrictEqual(false);
    });
  });

  describe("renameDocument feature flag", () => {
    test("Should return renameDocument feature false, if  FEATURE_FLAG_RENAME_DOCUMENT is false", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);

      mockConfig.FEATURE_FLAG_RENAME_DOCUMENT = false;
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.renameDocument).toStrictEqual(false);
    });

    test("Should return renameDocument feature true, if it is a cypress integration test user and does not have renameDocument=false in query param and FEATURE_FLAG_RENAME_DOCUMENT is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group1"],
          },
        },
      ]);
      window.Cypress = {};
      mockConfig.FEATURE_FLAG_RENAME_DOCUMENT = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.renameDocument).toStrictEqual(true);
    });

    test("Should return renameDocument feature true, if it is a cypress automation test(e2e) user, and does not have renameDocument=false in query param and FEATURE_FLAG_RENAME_DOCUMENT is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_NOTES = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.renameDocument).toStrictEqual(true);
    });

    test("Should return renameDocument feature false, if it is a cypress automation test(e2e) user, and have renameDocument=false in query param and FEATURE_FLAG_RENAME_DOCUMENT is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "private_beta_ignore_user",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group"],
          },
        },
      ]);
      window.Cypress = {};
      (
        useQueryParamsStateModule.useQueryParamsState as jest.Mock
      ).mockReturnValue({ renameDocument: "false" });
      mockConfig.PRIVATE_BETA_CHECK_IGNORE_USER = "private_beta_ignore_user";
      mockConfig.FEATURE_FLAG_RENAME_DOCUMENT = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.renameDocument).toStrictEqual(false);
    });

    test("Should return renameDocument feature true, if user is not in private beta feature groups and FEATURE_FLAG_RENAME_DOCUMENT is true", () => {
      (authModule.useUserDetails as jest.Mock).mockReturnValue({
        username: "test",
      });
      (
        msalInstanceModule.msalInstance.getAllAccounts as jest.Mock
      ).mockReturnValue([
        {
          idTokenClaims: {
            groups: ["private_beta_feature_group1"],
          },
        },
      ]);
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP = "private_beta_feature_group";
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      mockConfig.FEATURE_FLAG_RENAME_DOCUMENT = true;

      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.renameDocument).toStrictEqual(true);
    });
  });
});
