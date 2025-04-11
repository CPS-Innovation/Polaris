import {
  useAppInsightsTrackEvent,
  useAppInsightsTrackPageView,
} from "./useAppInsightsTracks";
import { renderHook } from "@testing-library/react";
import { MemoryRouter, useParams } from "react-router-dom";

jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useParams: jest.fn(),
}));
const mockTrackEvent = jest.fn();
const mockTrackPageView = jest.fn();
let mockUseAppInsightsContext = {
  trackEvent: mockTrackEvent,
  trackPageView: mockTrackPageView,
};

jest.mock("@microsoft/applicationinsights-react-js", () => ({
  useAppInsightsContext: () => mockUseAppInsightsContext,
}));
jest.mock("@microsoft/applicationinsights-react-js", () => ({
  useAppInsightsContext: () => mockUseAppInsightsContext,
}));

jest.mock("../../../app/auth/msal/useUserDetails", () => ({
  useUserDetails: () => ({
    name: "test_name",
    username: "test_username",
    cmsUserID: "test_cmsUserID",
  }),
}));

describe("useAppInsightsTracks hook", () => {
  beforeEach(() => {
    (useParams as jest.Mock).mockReturnValue({
      id: "testCaseId_1234",
      urn: "test-urn",
    });
  });
  afterEach(() => {
    mockTrackEvent.mockClear();
    mockTrackPageView.mockClear();
  });

  test("Should call the trackEvent with correct properties", () => {
    const { result } = renderHook(() => useAppInsightsTrackEvent(), {
      wrapper: MemoryRouter,
    });
    result.current("Search URN");
    expect(mockTrackEvent).toHaveBeenCalledTimes(1);
    expect(mockTrackEvent).toHaveBeenCalledWith({
      name: "Search URN",
      properties: {
        description:
          "User has clicked the 'Search' button on the 'Case Search' screen.",
        name: "test_name",
        username: "test_username",
        cmsUserID: "test_cmsUserID",
        caseId: "testCaseId_1234",
        urn: "test-urn",
      },
    });
  });

  test("Should not add the urn and caseId to rackEvent  properties if urn is not present through useParams", () => {
    (useParams as jest.Mock).mockReturnValue({});
    const { result } = renderHook(() => useAppInsightsTrackEvent(), {
      wrapper: MemoryRouter,
    });
    result.current("Search URN");
    expect(mockTrackEvent).toHaveBeenCalledTimes(1);
    expect(mockTrackEvent).toHaveBeenCalledWith({
      name: "Search URN",
      properties: {
        description:
          "User has clicked the 'Search' button on the 'Case Search' screen.",
        name: "test_name",
        username: "test_username",
        cmsUserID: "test_cmsUserID",
      },
    });
  });

  test("Should not call the mockTrackEvent if the name is empty", () => {
    const { result } = renderHook(() => useAppInsightsTrackEvent(), {
      wrapper: MemoryRouter,
    });
    result.current("" as any);
    expect(mockTrackEvent).toHaveBeenCalledTimes(0);
  });

  test("Should call the trackPageView with correct properties", () => {
    renderHook(() => useAppInsightsTrackPageView("Search Page View"), {
      wrapper: MemoryRouter,
    });
    expect(mockTrackPageView).toHaveBeenCalledTimes(1);
    expect(mockTrackPageView).toHaveBeenCalledWith({
      name: "Search Page View",
      properties: {
        name: "test_name",
        username: "test_username",
        cmsUserID: "test_cmsUserID",
      },
    });
  });

  test("Should not call the mockTrackPageView if the name is empty", () => {
    renderHook(() => useAppInsightsTrackPageView("" as any), {
      wrapper: MemoryRouter,
    });
    expect(mockTrackPageView).toHaveBeenCalledTimes(0);
  });

  test("Should not throw an error if appInsight is not initialized properly, when tracking PageView", () => {
    mockUseAppInsightsContext = undefined as any;
    expect(() =>
      renderHook(() => useAppInsightsTrackPageView("abc"), {
        wrapper: MemoryRouter,
      })
    ).not.toThrowError();
    expect(mockTrackPageView).toHaveBeenCalledTimes(0);
  });

  test("Should not throw error if appInsight is not initialized properly, when tracking an event", () => {
    mockUseAppInsightsContext = {
      trackEvent: undefined,
      trackPageView: undefined,
    } as any;
    const { result } = renderHook(() => useAppInsightsTrackEvent(), {
      wrapper: MemoryRouter,
    });
    expect(() => result.current("Search URN")).not.toThrowError();
    expect(mockTrackEvent).toHaveBeenCalledTimes(0);
  });
});
