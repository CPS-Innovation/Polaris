import "@testing-library/jest-dom";
import { renderHook } from "@testing-library/react";
import { useIsMounted } from "./useIsMounted";

describe("useIsMounted", () => {
  it("can tell you a component is mounted or unmounted", () => {
    const {
      result: { current: isMounted },
      unmount,
    } = renderHook(() => useIsMounted());

    expect(isMounted.current).toEqual(true);
    unmount();
    expect(isMounted.current).toEqual(false);
  });
});
