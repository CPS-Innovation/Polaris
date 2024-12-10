import { clearDownStorage, addToLocalStorage } from "./localStorageUtils";
jest.mock("../../../../../config", () => ({
  LOCAL_STORAGE_EXPIRY_DAYS: 1,
}));
describe("localStorageUtils", () => {
  const getAllLocalStorageKeys = () => {
    var keys = [];
    for (var i = 0; i < localStorage.length; i++) {
      keys.push(localStorage.key(i));
    }
    return keys;
  };
  describe("clearDownStorage", () => {
    afterEach(() => {
      localStorage.clear();
    });

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
          modifiedDate: Date.now() - oneDayMilliseconds + 100,
          value: "def",
        })
      );
      localStorage.setItem(
        "polaris-5",
        JSON.stringify({
          modifiedDate: Date.now() + 200,
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

      expect(getAllLocalStorageKeys()).toEqual([
        "test-1",
        "test-2",
        "polaris-4",
        "polaris-5",
      ]);
      expect(localStorage.length).toEqual(4);
    });
  });

  describe("addToLocalStorage", () => {
    afterEach(() => {
      localStorage.clear();
    });
    it("Should be able to add data to the localStorage with featurekey redactions", () => {
      addToLocalStorage(12, "redactions", "123");
      expect(localStorage.length).toEqual(1);
      expect(getAllLocalStorageKeys()).toEqual(["polaris-12"]);
      expect(JSON.parse(localStorage["polaris-12"]).redactions).toEqual("123");

      addToLocalStorage(12, "redactions", "124");
      expect(localStorage.length).toEqual(1);
      expect(getAllLocalStorageKeys()).toEqual(["polaris-12"]);
      expect(JSON.parse(localStorage["polaris-12"]).redactions).toEqual("124");

      addToLocalStorage(11, "redactions", "123");
      expect(localStorage.length).toEqual(2);
      expect(getAllLocalStorageKeys()).toEqual(["polaris-12", "polaris-11"]);

      expect(JSON.parse(localStorage["polaris-11"]).redactions).toEqual("123");
      expect(JSON.parse(localStorage["polaris-12"]).redactions).toEqual("124");
    });

    it("Should be able to add data to the localStorage with featurekey readUnread", () => {
      addToLocalStorage(12, "readUnread", "123");
      expect(localStorage.length).toEqual(1);
      expect(getAllLocalStorageKeys()).toEqual(["polaris-12"]);
      expect(JSON.parse(localStorage["polaris-12"]).readUnread).toEqual("123");

      addToLocalStorage(12, "readUnread", "124");
      expect(localStorage.length).toEqual(1);
      expect(getAllLocalStorageKeys()).toEqual(["polaris-12"]);
      expect(JSON.parse(localStorage["polaris-12"]).readUnread).toEqual("124");

      addToLocalStorage(11, "readUnread", "123");
      expect(localStorage.length).toEqual(2);
      expect(getAllLocalStorageKeys()).toEqual(["polaris-12", "polaris-11"]);

      expect(JSON.parse(localStorage["polaris-11"]).readUnread).toEqual("123");
      expect(JSON.parse(localStorage["polaris-12"]).readUnread).toEqual("124");
    });
  });
});
