import { getCookieValue } from "./getCookieValue";

describe("getCookieValue", () => {
  test("Should get the correct cookie value", () => {
    const cookieSpy = jest.spyOn(document, "cookie", "get");
    cookieSpy.mockReturnValue("UID=111; abc==123;");
    const value = getCookieValue("UID");
    expect(value).toEqual("111");
    cookieSpy.mockRestore();
  });
  test("Should return null if the cookie is not present", () => {
    const cookieSpy = jest.spyOn(document, "cookie", "get");
    cookieSpy.mockReturnValue("abc==123;");
    const value = getCookieValue("UID");
    expect(value).toEqual(null);
    cookieSpy.mockRestore();
  });
  test("Should trim all trailing and leading empty spaces", () => {
    const cookieSpy = jest.spyOn(document, "cookie", "get");
    cookieSpy.mockReturnValue("   UID=111   ; abc==123;");
    const value = getCookieValue("UID");
    expect(value).toEqual("111");
    cookieSpy.mockRestore();
  });
});
