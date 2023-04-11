import { generateGuid } from "./generate-guid";

describe("generateGuid", () => {
  it("can generate a valid guid", () => {
    expect(generateGuid()).toMatch(
      /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i
    );
  });

  it("can return the guid that has been set against the window", () => {
    const guid = "C381AD9A-10AE-48F2-86E4-4C4730C0749A";
    window.__POLARIS_INSTRUMENTATION_GUID__ = guid;

    expect(generateGuid()).toEqual(guid);
  });

  it("can return the guid that has been set against the window when the guid has been reset", () => {
    const guid = "C381AD9A-10AE-48F2-86E4-4C4730C0749A";
    window.__POLARIS_INSTRUMENTATION_GUID__ = guid;

    const firstResult = generateGuid();
    const secondResult = generateGuid();
    expect(firstResult).toEqual(guid);
    expect(secondResult).toEqual(guid);

    const guid2 = "4A6983E5-D9C2-43A4-BFD6-3763303D5A0C";
    window.__POLARIS_INSTRUMENTATION_GUID__ = guid2;

    expect(generateGuid()).toEqual(guid2);
  });
});
