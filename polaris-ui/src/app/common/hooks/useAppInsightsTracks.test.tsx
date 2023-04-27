import {
  useAppInsightsTrackEvent,
  useAppInsightsTrackPageView,
} from "./useAppInsightsTracks";
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
    const { result } = renderHook(() => useAppInsightsTrackEvent());
    result.current("Search URN");
    expect(mockTrackEvent).toHaveBeenCalledTimes(1);
    expect(mockTrackEvent).toHaveBeenCalledWith({
      name: "Search URN",
      properties: {
        description:
          "User has clicked the 'Search' button on the 'Find a case' screen.",
      },
    });
  });

  test("Should not call the mockTrackEvent if the name is empty", () => {
    const { result } = renderHook(() => useAppInsightsTrackEvent());
    result.current("" as any);
    expect(mockTrackEvent).toHaveBeenCalledTimes(0);
  });

  test("Should call the trackPageView with correct properties", () => {
    renderHook(() => useAppInsightsTrackPageView("Search Page View"));
    expect(mockTrackPageView).toHaveBeenCalledTimes(1);
    expect(mockTrackPageView).toHaveBeenCalledWith({
      name: "Search Page View",
    });
  });

  test("Should not call the mockTrackPageView if the name is empty", () => {
    renderHook(() => useAppInsightsTrackPageView("" as any));
    expect(mockTrackPageView).toHaveBeenCalledTimes(0);
  });

  test("Should not throw error if appInsight is not initialized properly, when tracking PageView", () => {
    mockUseAppInsightsContext = undefined as any;
    expect(() =>
      renderHook(() => useAppInsightsTrackPageView("abc"))
    ).not.toThrowError();
    expect(mockTrackPageView).toHaveBeenCalledTimes(0);
  });

  test("Should not throw error if appInsight is not initialized properly, when tracking Event", () => {
    mockUseAppInsightsContext = {
      trackEvent: undefined,
      trackPageView: undefined,
    } as any;
    const { result } = renderHook(() => useAppInsightsTrackEvent());
    expect(() => result.current("Search URN")).not.toThrowError();
    expect(mockTrackEvent).toHaveBeenCalledTimes(0);
  });
});
