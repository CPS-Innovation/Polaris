import { isPathMatch } from "./is-path-match";

type Config = Parameters<typeof isPathMatch>[0];

beforeEach(() => {
  jest.spyOn(console, "debug").mockImplementation(jest.fn());
});

describe("isPathMatch", () => {
  it.each([
    ["http://foo.com", "/", "http://foo.com/", "/", true],
    ["http://foo.com/bar", "/", "http://foo.com/bar", "/bar", true],
    ["http://foo.com/bar", "/", "http://foo.com/bar/baz", "/bar/baz", true],
    ["http://foo.com/bar", "/(?<tag>baz)", "http://foo.com/bar/baz", "/bar/baz", true],
    ["http://foo.com/bar", "/(?<tag>baz)/buzz", "http://foo.com/bar/baz/buzz", "/bar/baz/buzz", true],
    ["http://foo.com/bar", "/(?<tag>baz)/(?<tag2>buzz)", "http://foo.com/bar/baz/buzz/qux", "/bar/baz/buzz/qux", true],
    ["http://bar.com/bar", "/(?<tag>baz)/buzz", "http://foo.com/bar/baz/buzz", "/bar/baz/buzz", false],
    ["http://foo.com/bar", "/(?<tag>buzz)", "http://foo.com/bar/baz", "/bar/baz", false],
  ])("will match paths", (pathRoot: string, path: string, href: string, fullPath: string, shouldMatch: boolean) => {
    const config = { pathRoot, path, pathMatcherValues: {} } as Config;
    expect(isPathMatch(config, href, fullPath)).toBe(shouldMatch);
  });
});
