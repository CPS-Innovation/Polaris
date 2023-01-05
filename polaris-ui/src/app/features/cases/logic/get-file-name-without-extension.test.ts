import { getFileNameWithoutExtension } from "./get-file-name-without-extension";

describe("getFileNameWithoutExtension", () => {
  it("can remove the extension from a filename", () => {
    expect(getFileNameWithoutExtension("foo.txt")).toBe("foo");
  });

  it("can remove the extension from a filename with many dots", () => {
    expect(getFileNameWithoutExtension("foo.bar.baz.txt")).toBe("foo.bar.baz");
  });

  it("can cope with a filename with no extension", () => {
    expect(getFileNameWithoutExtension("foo")).toBe("foo");
  });

  it("can cope with a filename with a single letter extension", () => {
    expect(getFileNameWithoutExtension("foo.t")).toBe("foo");
  });

  it("can cope with a filename with a letter name", () => {
    expect(getFileNameWithoutExtension("f.txt")).toBe("f");
  });

  it("can cope with a dotfile", () => {
    expect(getFileNameWithoutExtension(".settings")).toBe(".settings");
  });

  it("can cope with an empty filename", () => {
    expect(getFileNameWithoutExtension("")).toBe("");
  });

  it("can return empty string for an undefined filename", () => {
    expect(getFileNameWithoutExtension(undefined)).toBe("");
  });
});
