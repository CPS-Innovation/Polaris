import { clearDownStorage } from "./localStorageUtils";
jest.mock("../../../../../config", () => ({
  LOCAL_STORAGE_EXPIRY_DAYS: 1,
}));
describe("localStorageUtils", () => {
  describe("clearDownStorage", () => {
    afterEach(() => {
      localStorage.clear();
    });

    const getAllLocalStorageKeys = () => {
      var keys = [];
      for (var i = 0; i < localStorage.length; i++) {
        keys.push(localStorage.key(i));
      }
      return keys;
    };

    it("Should clear down only the expired localstorage items with keys contains text polaris-", () => {
      const oneDayMilliseconds = 24 * 60 * 60 * 1000;
      localStorage.setItem(
        "test-1",
        JSON.stringify({
          modifiedDate: Date.now() - oneDayMilliseconds,
          value: "abc",
        })
      );
      localStorage.setItem("test-2", "abc");

      localStorage.setItem(
        "polaris-1",
        JSON.stringify({
          modifiedDate: Date.now() - oneDayMilliseconds,
          value: "abc",
        })
      );
      localStorage.setItem(
        "polaris-2",
        JSON.stringify({
          modifiedDate: Date.now() - oneDayMilliseconds - 200,
          value: "abc",
        })
      );

      localStorage.setItem(
        "polaris-3",
        JSON.stringify({
          modifiedDate: Date.now() - oneDayMilliseconds - 100,
          value: "abc",
        })
      );
      localStorage.setItem(
        "polaris-4",
        JSON.stringify({
          modifiedDate: Date.now() - oneDayMilliseconds + 10,
          value: "def",
        })
      );
      localStorage.setItem(
        "polaris-5",
        JSON.stringify({
          modifiedDate: Date.now(),
          value: "def",
        })
      );
      expect(localStorage.length).toEqual(7);
      expect(getAllLocalStorageKeys()).toEqual([
        "test-1",
        "test-2",
        "polaris-1",
        "polaris-2",
        "polaris-3",
        "polaris-4",
        "polaris-5",
      ]);
      clearDownStorage();

      expect(localStorage.length).toEqual(4);
      expect(getAllLocalStorageKeys()).toEqual([
        "test-1",
        "test-2",
        "polaris-4",
        "polaris-5",
      ]);
    });
  });
});
