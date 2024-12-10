import {
  saveStateToSessionStorage,
  getStateFromSessionStorage,
} from "./stateRetentionUtil";
import { CombinedState } from "../../../domain/CombinedState";
describe("StateRetentionUtils", () => {
  describe("saveStateToSessionStorage", () => {
    afterEach(() => {
      jest.clearAllMocks();
    });

    it("should save serialized state to sessionStorage", () => {
      Object.defineProperty(window, "sessionStorage", {
        value: {
          setItem: jest.fn(),
        },
        writable: true,
      });
      const mockState = { caseId: "123", data: "test data" };
      const serializedState = JSON.stringify(mockState);
      saveStateToSessionStorage(mockState as unknown as CombinedState);
      expect(sessionStorage.setItem).toHaveBeenCalledWith(
        "casework_app_state_123",
        serializedState
      );
    });

    it("should handle errors and log to console", () => {
      const mockState = { caseId: "456", data: "test data" };
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
    });
  });

  describe("getStateFromSessionStorage", () => {
    afterEach(() => {
      jest.clearAllMocks();
    });
    it("Should successfully retrieve state from the sessionStorage", () => {
      Object.defineProperty(window, "sessionStorage", {
        value: {
          getItem: jest.fn().mockImplementation(() =>
            JSON.stringify({
              id: "1",
            })
          ),
        },
      });
      const state = getStateFromSessionStorage(1);
      expect(sessionStorage.getItem).toHaveBeenCalledWith(
        "casework_app_state_1"
      );
      expect(state).toEqual({ id: "1" });
    });

    it("Should successfully return  null if it cant retrieve state from sessionStorage", () => {
      Object.defineProperty(window, "sessionStorage", {
        value: {
          getItem: jest.fn().mockImplementation(() => undefined),
        },
      });
      const consoleErrorSpy = jest
        .spyOn(console, "error")
        .mockImplementation(() => {});
      const state = getStateFromSessionStorage(1);
      expect(sessionStorage.getItem).toHaveBeenCalledWith(
        "casework_app_state_1"
      );
      expect(state).toEqual(null);
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        "Could not retrieve state from sessionStorage:",
        expect.any(Error)
      );
      consoleErrorSpy.mockRestore();
    });
  });
});
