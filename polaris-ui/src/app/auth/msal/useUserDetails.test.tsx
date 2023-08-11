import { useUserDetails } from "./useUserDetails";

jest.mock("./msalInstance", () => ({
  msalInstance: {
    getAllAccounts: () => [
      {
        username: "test_username",
        name: "test_name",
      },
    ],
  },
}));

describe("getUserDetails", () => {
  test("Should give the correct user details properties", () => {
    const cookieSpy = jest.spyOn(document, "cookie", "get");
    cookieSpy.mockReturnValue("UID=test_UID; abc=123;");

    const userDetails = useUserDetails();
    expect(userDetails).toEqual({
      cmsUserID: "test_UID",
      name: "test_name",
      username: "test_username",
    });
    cookieSpy.mockRestore();
  });

  test("Should not throw error if UID property is not present in the cookie", () => {
    const cookieSpy = jest.spyOn(document, "cookie", "get");
    cookieSpy.mockReturnValue("test_UID; abc=123;");

    const userDetails = useUserDetails();
    expect(userDetails).toEqual({
      cmsUserID: null,
      name: "test_name",
      username: "test_username",
    });
    cookieSpy.mockRestore();
  });
});
