import * as api from "../../api/gateway-api";
import { searchCaseWhenReady } from "./search-case-when-ready";

describe("searchCaseWhenReady", () => {
  it("can not search when there is no search term", async () => {
    const result = await searchCaseWhenReady("", 1, "", true, true);

    expect(result).toBeUndefined();
  });

  it("can not search when the pipeline is not complete", async () => {
    const result = await searchCaseWhenReady("", 1, "foo", false, true);

    expect(result).toBeUndefined();
  });

  it("can not search when the documents call is not complete", async () => {
    const result = await searchCaseWhenReady("", 1, "foo", true, false);

    expect(result).toBeUndefined();
  });

  it("can search when ready", async () => {
    const searchCaseSpy = jest
      .spyOn(api, "searchCase")
      .mockImplementation(() => Promise.resolve([]));

    const result = await searchCaseWhenReady("", 1, "foo", true, true);

    expect(result).toEqual([]);
    expect(searchCaseSpy).toBeCalled();
  });
});
