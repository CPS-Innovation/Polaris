import { sanitizeSearchTerm } from "./sanitizeSearchTerm";

describe("sanitizeSearchTerm", () => {
  it.each([
    ["", ""],
    ["foo", "foo"],
    [" foo", "foo"],
    ["  foo", "foo"],
    ["foo ", "foo"],
    ["foo  ", "foo"],
    ["foo bar", "foo"],
    [" foo bar", "foo"],
    ["foo bar ", "foo"],
    ["foo bar baz", "foo"],
    ["  foo        bar  ", "foo"],
  ])("can return expected results", (input, expected) => {
    expect(sanitizeSearchTerm(input)).toBe(expected);
  });
});
