import { buildSortedFullPath } from "./build-sorted-full-path";

describe("buildSortedFullPath", () => {
  it("returns a full path with no query", () => {
    expect(buildSortedFullPath({ pathname: "/foo/bar" } as Location)).toEqual("/foo/bar");
  });

  it("returns a full path with a single query param", () => {
    expect(buildSortedFullPath({ pathname: "/foo/bar", search: "?baz=1" } as Location)).toEqual("/foo/bar?baz=1");
  });

  it("returns a full path with multiple query params, with the params being sorted alphabetically", () => {
    expect(buildSortedFullPath({ pathname: "/foo/bar", search: "?baz=1&dog=2&cat=3&baz=2" } as Location)).toEqual("/foo/bar?baz=1&baz=2&cat=3&dog=2");
  });
});
