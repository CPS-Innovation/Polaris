import "@testing-library/jest-dom";
import "@testing-library/jest-dom/extend-expect";
import { renderHook } from "@testing-library/react-hooks";
import { useIsMounted } from "./useIsMounted";

describe("useIsMounted", () => {
  it("can tell you a component is mounted or unmounted", () => {
    const {
      result: { current: isMounted },
      unmount,
    } = renderHook(() => useIsMounted());

    expect(isMounted()).toEqual(true);
    unmount();
    expect(isMounted()).toEqual(false);
  });
});
