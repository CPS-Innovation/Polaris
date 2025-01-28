import { AppLocationConfig, PathMatcher } from "../../LocationConfig";
import { flattenConfig } from "./flatten-config";

describe("flattenConfig", () => {
  it("flattens config", () => {
    const config: AppLocationConfig[] = [
      {
        pathRoots: ["http://a", "http://b"],
        pathMatchers: [
          {
            paths: ["/c", "/d"],
            matchedLinkCode: "cases",
          } as PathMatcher,
          {
            paths: ["/e", "/f"],
            matchedLinkCode: "tasks",
          } as PathMatcher,
        ],
      },
      {
        pathRoots: ["http://g", "http://h"],
        pathMatchers: [
          {
            paths: ["/i", "/j"],
            matchedLinkCode: "tasks",
          } as PathMatcher,
          {
            paths: ["/k", "/l"],
            matchedLinkCode: "cases",
          } as PathMatcher,
        ],
      },
    ];

    expect(flattenConfig(config)).toEqual([
      { pathRoot: "http://a", path: "/c", pathMatcherValues: { matchedLinkCode: "cases" } },
      { pathRoot: "http://a", path: "/d", pathMatcherValues: { matchedLinkCode: "cases" } },
      { pathRoot: "http://a", path: "/e", pathMatcherValues: { matchedLinkCode: "tasks" } },
      { pathRoot: "http://a", path: "/f", pathMatcherValues: { matchedLinkCode: "tasks" } },
      { pathRoot: "http://b", path: "/c", pathMatcherValues: { matchedLinkCode: "cases" } },
      { pathRoot: "http://b", path: "/d", pathMatcherValues: { matchedLinkCode: "cases" } },
      { pathRoot: "http://b", path: "/e", pathMatcherValues: { matchedLinkCode: "tasks" } },
      { pathRoot: "http://b", path: "/f", pathMatcherValues: { matchedLinkCode: "tasks" } },

      { pathRoot: "http://g", path: "/i", pathMatcherValues: { matchedLinkCode: "tasks" } },
      { pathRoot: "http://g", path: "/j", pathMatcherValues: { matchedLinkCode: "tasks" } },
      { pathRoot: "http://g", path: "/k", pathMatcherValues: { matchedLinkCode: "cases" } },
      { pathRoot: "http://g", path: "/l", pathMatcherValues: { matchedLinkCode: "cases" } },
      { pathRoot: "http://h", path: "/i", pathMatcherValues: { matchedLinkCode: "tasks" } },
      { pathRoot: "http://h", path: "/j", pathMatcherValues: { matchedLinkCode: "tasks" } },
      { pathRoot: "http://h", path: "/k", pathMatcherValues: { matchedLinkCode: "cases" } },
      { pathRoot: "http://h", path: "/l", pathMatcherValues: { matchedLinkCode: "cases" } },
    ] as ReturnType<typeof flattenConfig>);
  });
});
