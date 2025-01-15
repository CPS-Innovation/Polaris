import {
  saveStateToSessionStorage,
  getStateFromSessionStorage,
} from "./stateRetentionUtil";
import { CombinedState } from "../../../domain/CombinedState";
describe("StateRetentionUtils", () => {
  describe("saveStateToSessionStorage", () => {
    beforeEach(() => {
      jest.spyOn(console, "error").mockImplementation(() => {});
    });
    afterEach(() => {
      jest.clearAllMocks();
    });

    it("should save serialized state to sessionStorage", () => {
      const mockState = { caseId: "123", data: "test data" };
      saveStateToSessionStorage(mockState as unknown as CombinedState);
      const storedState = sessionStorage.getItem(
        `casework_app_state_${mockState.caseId}`
      );
      expect(storedState).toBe(JSON.stringify(mockState));
    });

    it("should handle errors and log to console", () => {
      const mockState = { caseId: "456", data: "test data" };
      const originalSessionStorage = window.sessionStorage;
      Object.defineProperty(window, "sessionStorage", {
        value: {
          setItem: jest.fn(() => {
            throw new Error("Storage error");
          }),
        },
        writable: true,
      });

      const consoleErrorSpy = jest
        .spyOn(console, "error")
        .mockImplementation(() => {});
      saveStateToSessionStorage(mockState as unknown as CombinedState);
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        "Could not save state to sessionStorage:",
        expect.any(Error)
      );
      consoleErrorSpy.mockRestore();
      Object.defineProperty(window, "sessionStorage", {
        value: originalSessionStorage,
      });
    });
  });

  describe("getStateFromSessionStorage", () => {
    const mockState = { caseId: 123, data: { name: "test" } };
    const sessionStorageKey = `casework_app_state_${mockState.caseId}`;

    beforeEach(() => {
      jest.spyOn(console, "error").mockImplementation(() => {});
      sessionStorage.clear();
    });

    afterEach(() => {
      jest.restoreAllMocks();
    });

    it("should retrieve and parse the state from sessionStorage", () => {
      sessionStorage.setItem(sessionStorageKey, JSON.stringify(mockState));
      const result = getStateFromSessionStorage(mockState.caseId);
      expect(result).toEqual(mockState);
    });

    it("should return null if the state does not exist in sessionStorage", () => {
      const result = getStateFromSessionStorage(mockState.caseId);
      expect(result).toBeNull();
    });

    it("should handle invalid JSON and return null", () => {
      sessionStorage.setItem(sessionStorageKey, "{ invalid json }");
      const result = getStateFromSessionStorage(mockState.caseId);
      expect(result).toBeNull();
      expect(console.error).toHaveBeenCalledWith(
        "Could not retrieve state from sessionStorage:",
        expect.any(Error)
      );
    });

    it("should remove keys with prefix except the specified one", () => {
      sessionStorage.setItem(
        "casework_app_state_123",
        JSON.stringify(mockState)
      );
      sessionStorage.setItem(
        "casework_app_state_456",
        JSON.stringify({ caseId: 67890 })
      );
      sessionStorage.setItem(
        "casework_app_state_789",
        JSON.stringify({ caseId: 0 })
      );
      expect(sessionStorage.getItem("casework_app_state_456")).not.toBeNull();
      expect(sessionStorage.getItem("casework_app_state_789")).not.toBeNull();
      const result = getStateFromSessionStorage(123);

      expect(result).toEqual(mockState);
      expect(sessionStorage.getItem("casework_app_state_456")).toBeNull();
      expect(sessionStorage.getItem("casework_app_state_789")).toBeNull();
    });

    it("should log an error and return null if an exception occurs", () => {
      const originalSessionStorage = window.sessionStorage;
      Object.defineProperty(window, "sessionStorage", {
        value: {
          getItem: jest.fn(() => {
            throw new Error("Storage error");
          }),
        },
      });
      const result = getStateFromSessionStorage(mockState.caseId);

      expect(result).toBeNull();
      expect(console.error).toHaveBeenCalledWith(
        "Could not retrieve state from sessionStorage:",
        expect.any(Error)
      );

      Object.defineProperty(window, "sessionStorage", {
        value: originalSessionStorage,
      });
    });
  });
});
