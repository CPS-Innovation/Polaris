import { renderHook, act } from "@testing-library/react-hooks";
import { useSearchInputLogic } from "./useSearchInputLogic";
import * as isUrnValid from "../logic/is-urn-valid";

let mockIsValidValue: boolean;

describe("useSearchInputLogic", () => {
  beforeEach(() => {
    jest
      .spyOn(isUrnValid, "isUrnValid")
      .mockImplementation((urn: string) => mockIsValidValue);
  });

  it("can set URN values", async () => {
    const { result } = renderHook(() =>
      useSearchInputLogic({ initialUrn: "A", setParams: (params) => {} })
    );

    expect(result.current.urn).toBe("A");

    act(() => result.current.handleChange("b"));

    expect(result.current.urn).toBe("B");
  });

  it("can initialise with an undefined inittialUrn", () => {
    const { result } = renderHook(() =>
      useSearchInputLogic({ initialUrn: undefined, setParams: (params) => {} })
    );

    expect(result.current.urn).toBe("");
  });

  it("can submit a valid urn and call setParams", () => {
    const mockSetParams = jest.fn();
    mockIsValidValue = true;

    const { result } = renderHook(() =>
      useSearchInputLogic({ initialUrn: "A", setParams: mockSetParams })
    );

    act(() => result.current.handleSubmit());

    expect(result.current.isError).toBe(false);
    expect(mockSetParams).toBeCalledWith({ urn: "A" });
  });

  it("can submit an invalid urn and not call setParams", () => {
    const mockSetParams = jest.fn();
    mockIsValidValue = false;

    const { result } = renderHook(() =>
      useSearchInputLogic({ initialUrn: "A", setParams: mockSetParams })
    );

    act(() => result.current.handleSubmit());

    expect(result.current.isError).toBe(true);
    expect(mockSetParams).not.toHaveBeenCalled();
  });

  it("can submit with by pressing enter", () => {
    const mockSetParams = jest.fn();
    mockIsValidValue = true;

    const { result } = renderHook(() =>
      useSearchInputLogic({ initialUrn: "A", setParams: mockSetParams })
    );

    act(() =>
      result.current.handleKeyPress({
        key: "Enter",
      } as React.KeyboardEvent<HTMLInputElement>)
    );

    expect(mockSetParams).toBeCalledWith({ urn: "A" });
  });

  it("can not submit with by pressing space", () => {
    const mockSetParams = jest.fn();
    mockIsValidValue = true;

    const { result } = renderHook(() =>
      useSearchInputLogic({ initialUrn: "A", setParams: mockSetParams })
    );

    act(() =>
      result.current.handleKeyPress({
        key: "Space",
      } as React.KeyboardEvent<HTMLInputElement>)
    );

    expect(mockSetParams).not.toBeCalled();
  });
});
