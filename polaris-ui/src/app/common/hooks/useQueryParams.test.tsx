import { renderHook, act } from "@testing-library/react-hooks";
import { useQueryParamsState } from "./useQueryParamsState";
import { createMemoryHistory } from "history";
import { Router } from "react-router-dom";

const history = createMemoryHistory();

describe("useQueryParams", () => {
  test("reads single params", () => {
    history.push("/my/route?foo=a&bar=b");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string; bar: string }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    const { foo, bar } = result.current;

    expect({ foo, bar }).toEqual({
      foo: "a",
      bar: "b",
    });
  });

  test("reads array params", () => {
    history.push("/my/route?foo=a,b");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string[] }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    const { foo } = result.current;

    expect({ foo }).toEqual({
      foo: ["a", "b"],
    });
  });

  test("reads empty params", () => {
    history.push("/my/route?foo=");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    const { foo } = result.current;

    expect({ foo }).toEqual({
      foo: "",
    });
  });

  test("sets single params", () => {
    history.push("/my/route");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    act(() => result.current.setParams({ foo: "a" }));

    expect(history.location.search).toBe("?foo=a");
  });

  test("sets many single params", () => {
    history.push("/my/route");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string; bar: string }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    act(() => result.current.setParams({ foo: "a", bar: "b" }));

    expect(history.location.search).toBe("?foo=a&bar=b");
  });

  test("sets array params with a single value", () => {
    history.push("/my/route");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string[] }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    act(() => result.current.setParams({ foo: ["a"] }));

    expect(history.location.search).toBe("?foo=a");
  });

  test("sets array params with a many values", () => {
    history.push("/my/route");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string[] }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    act(() => result.current.setParams({ foo: ["a", "b"] }));

    expect(history.location.search).toBe("?foo=a,b");
  });

  test("sets array params with no values", () => {
    history.push("/my/route");
    const { result } = renderHook(
      () => useQueryParamsState<{ foo: string[] }>(),
      {
        wrapper: ({ children }) => (
          <Router history={history}>{children}</Router>
        ),
      }
    );

    act(() => result.current.setParams({ foo: [] }));

    expect(history.location.search).toBe("");
  });
});
