import { MatchedPathMatcher, OnwardLinkDefinitions } from "../../LocationConfig";
import { replaceTagsInLinks } from "./replace-tags-in-links";

beforeEach(() => {
  jest.spyOn(console, "debug").mockImplementation(jest.fn());
});

describe("replaceTagsInLinks", () => {
  it.each([[null, undefined, {}]])("returns the original links if pathTags is empty", (pathTags: MatchedPathMatcher["pathTags"]) => {
    const linkDefinitions = {} as OnwardLinkDefinitions;
    expect(replaceTagsInLinks(linkDefinitions, pathTags)).toBe(linkDefinitions);
  });

  it("replaces tags in link definitions", () => {
    expect(replaceTagsInLinks({ cases: "a={foo}&b={bar}", tasks: "{bar}/{foo}", details: "foo/bar" }, { foo: "AAA", bar: "BBB", baz: "CCC" })).toEqual({
      cases: "a=AAA&b=BBB",
      tasks: "BBB/AAA",
      details: "foo/bar",
    });
  });
});
