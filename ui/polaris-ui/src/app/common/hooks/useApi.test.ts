import { renderHook } from "@testing-library/react-hooks";
import { useApi } from "./useApi";

describe("useApi", () => {
  it("can initiate a call, set status to loading, then return a successful result", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = jest.fn(
      (id: string) =>
        new Promise((resolve) => setTimeout(() => resolve(mockResult), 10))
    );

    const { result, waitForNextUpdate } = renderHook(() =>
      useApi(mockApiCall, "1")
    );

    expect(result.current).toEqual({ status: "loading" });
    await waitForNextUpdate();
    expect(result.current).toEqual({ status: "succeeded", data: mockResult });
  });

  it("can initiate a call, set status to loading, then return an error result", async () => {
    const mockError = new Error();
    const mockApiCall = jest.fn(
      (id: string) =>
        new Promise((_, reject) => setTimeout(() => reject(mockError)))
    );

    const { result, waitForNextUpdate } = renderHook(() =>
      useApi(mockApiCall, "1")
    );

    expect(result.current).toEqual({ status: "loading" });
    await waitForNextUpdate();
    expect(result.current).toEqual({ status: "failed", error: mockError });
  });

  it("can initiate a call with multiple parameters", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = jest.fn(
      (p1: string, p2: number, p3: string) =>
        new Promise((resolve) => setTimeout(() => resolve(mockResult), 10))
    );

    const { result, waitForNextUpdate } = renderHook(() =>
      useApi(mockApiCall, "1", 2, "3")
    );

    expect(result.current).toEqual({ status: "loading" });
    await waitForNextUpdate();
    expect(result.current).toEqual({
      status: "succeeded",
      data: mockResult,
    });
  });

  it("can not call the api again if parameters do not change", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = jest.fn(
      (p1: string) =>
        new Promise((resolve) => setTimeout(() => resolve(mockResult), 10))
    );

    const { rerender } = renderHook(() => useApi(mockApiCall, "1"));

    expect(mockApiCall.mock.calls.length).toEqual(1);
    rerender();
    expect(mockApiCall.mock.calls.length).toEqual(1);
  });

  it("can call the api a second time if parameters do change", async () => {
    const mockResult = { id: 1 };
    const mockApiCall = jest.fn(
      (p1: string) =>
        new Promise((resolve) => setTimeout(() => resolve(mockResult), 10))
    );

    const { rerender } = renderHook(({ del, p1 }) => useApi(del, p1), {
      initialProps: {
        del: mockApiCall,
        p1: "1",
      },
    });

    expect(mockApiCall.mock.calls.length).toEqual(1);

    rerender({
      del: mockApiCall,
      p1: "2",
    });

    expect(mockApiCall.mock.calls.length).toEqual(2);
  });
});
