import { useAppInsightsTracks } from "./useAppInsightsTracks";
import { renderHook } from "@testing-library/react-hooks";
const mockTrackEvent = jest.fn();
const mockTrackPageView = jest.fn();
let mockUseAppInsightsContext = {
  trackEvent: mockTrackEvent,
  trackPageView: mockTrackPageView,
};
jest.mock("@microsoft/applicationinsights-react-js", () => ({
  useAppInsightsContext: () => mockUseAppInsightsContext,
}));

describe("useAppInsightsTracks hook", () => {
  afterEach(() => {
    mockTrackEvent.mockClear();
    mockTrackPageView.mockClear();
  });

  test("Should call the trackEvent with correct properties", () => {
    const { result } = renderHook(() => useAppInsightsTracks());
    result.current.trackEvent("Search URN");
    expect(mockTrackEvent).toHaveBeenCalledTimes(1);
    expect(mockTrackEvent).toHaveBeenCalledWith({
      name: "Search URN",
      properties: {
        description:
          "User has clicked the 'Search' button on the 'Find a case' screen.",
      },
    });
  });

  test("Should call the trackPageView with correct properties", () => {
    const { result } = renderHook(() => useAppInsightsTracks());
    result.current.trackPageView("Search Page View");
    expect(mockTrackPageView).toHaveBeenCalledTimes(1);
    expect(mockTrackPageView).toHaveBeenCalledWith({
      name: "Search Page View",
    });
  });

  test("Should not call the mockTrackEvent if the name is empty", () => {
    const { result } = renderHook(() => useAppInsightsTracks());
    result.current.trackEvent("" as any);
    expect(mockTrackEvent).toHaveBeenCalledTimes(0);
  });

  test("Should not call the mockTrackPageView if the name is empty", () => {
    const { result } = renderHook(() => useAppInsightsTracks());
    result.current.trackPageView("" as any);
    expect(mockTrackPageView).toHaveBeenCalledTimes(0);
  });

  test("Should not throw error if appInsight is not initialized properly, when tracking PageView", () => {
    mockUseAppInsightsContext = undefined as any;
    const { result } = renderHook(() => useAppInsightsTracks());
    result.current.trackPageView("abc");
    expect(() => result.current.trackPageView("abc")).not.toThrowError();
    expect(mockTrackPageView).toHaveBeenCalledTimes(0);
  });

  test("Should not throw error if appInsight is not initialized properly, when tracking Event", () => {
    mockUseAppInsightsContext = {
      trackEvent: undefined,
      trackPageView: undefined,
    } as any;
    const { result } = renderHook(() => useAppInsightsTracks());
    expect(() => result.current.trackEvent("Search URN")).not.toThrowError();
    expect(mockTrackEvent).toHaveBeenCalledTimes(0);
  });
});
