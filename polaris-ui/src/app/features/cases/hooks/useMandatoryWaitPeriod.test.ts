import { act, renderHook } from "@testing-library/react-hooks";
import { useMandatoryWaitPeriod } from "./useMandatoryWaitPeriod";

jest.useFakeTimers();

const PAUSE_PERIOD_MS = 1000;
const MANDATORY_WAIT_PERIOD = 1000;

describe("useMandatoryWaitPeriod", () => {
  it("can be immediately ready if the external flag is initially true", () => {
    const { result } = renderHook(() =>
      useMandatoryWaitPeriod(true, PAUSE_PERIOD_MS, MANDATORY_WAIT_PERIOD)
    );

    expect(result.current).toBe("ready");

    act(() => {
      jest.advanceTimersByTime(PAUSE_PERIOD_MS + 100);
    });

    expect(result.current).toBe("ready");

    act(() => {
      jest.advanceTimersByTime(MANDATORY_WAIT_PERIOD);
    });

    expect(result.current).toBe("ready");
  });

  it("can be ready before the pre-wait period has elapsed and not force the user to wait", () => {
    let isReady = false;
    const { result, rerender } = renderHook(() =>
      useMandatoryWaitPeriod(isReady, PAUSE_PERIOD_MS, MANDATORY_WAIT_PERIOD)
    );

    expect(result.current).toBe("preWait");

    isReady = true;
    rerender();

    expect(result.current).toBe("ready");

    act(() => {
      jest.advanceTimersByTime(PAUSE_PERIOD_MS + 100);
    });

    expect(result.current).toBe("ready");

    act(() => {
      jest.advanceTimersByTime(MANDATORY_WAIT_PERIOD);
    });

    expect(result.current).toBe("ready");
  });

  it("can be ready during the the mandatory wait period and forces the user to wait until the wait period is over", () => {
    let isReady = false;
    const { result, rerender } = renderHook(() =>
      useMandatoryWaitPeriod(isReady, PAUSE_PERIOD_MS, MANDATORY_WAIT_PERIOD)
    );

    act(() => {
      jest.advanceTimersByTime(PAUSE_PERIOD_MS + 100);
    });

    expect(result.current).toBe("wait");

    isReady = true;
    rerender();

    expect(result.current).toBe("wait");

    act(() => {
      jest.advanceTimersByTime(MANDATORY_WAIT_PERIOD);
    });

    expect(result.current).toBe("ready");
  });

  it("can be ready if the external flag is true some time after the mandatory wait period", () => {
    let isReady = false;
    const { result, rerender } = renderHook(() =>
      useMandatoryWaitPeriod(isReady, PAUSE_PERIOD_MS, MANDATORY_WAIT_PERIOD)
    );

    act(() => {
      jest.advanceTimersByTime(PAUSE_PERIOD_MS + 100);
    });

    expect(result.current).toBe("wait");

    act(() => {
      jest.advanceTimersByTime(MANDATORY_WAIT_PERIOD);
    });

    expect(result.current).toBe("wait");

    isReady = true;
    rerender();

    expect(result.current).toBe("ready");
  });

  it("can be reset by the external flag going back to false", () => {
    let isReady = true;
    const { result, rerender } = renderHook(() =>
      useMandatoryWaitPeriod(isReady, PAUSE_PERIOD_MS, MANDATORY_WAIT_PERIOD)
    );

    expect(result.current).toBe("ready");

    isReady = false;
    rerender();

    expect(result.current).toBe("preWait");

    act(() => {
      jest.advanceTimersByTime(PAUSE_PERIOD_MS + 100);
    });

    expect(result.current).toBe("wait");
  });
});
