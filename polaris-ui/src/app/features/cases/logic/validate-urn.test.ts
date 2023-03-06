import { validateUrn } from "./validate-urn";

describe("urnRegex", () => {
  it.each([
    // dd/aa/ddddddd - traditional
    ["11AA2222233", "11AA2222233"],

    // dd/ad/ddddddd - commonly seen
    ["11Z12222233", "11Z12222233"],

    // dd/dd/ddddddd - seen in e.g. NCA cases
    ["11992222233", "11992222233"],

    // dd/da/ddddddd - seen in data dump or URNs from Jake, may or may not be a typo in data
    //  but we will allow it at the moment
    ["111X2222233", "111X2222233"],

    // now test the suffix variants
    //  can have /1 or /11 suffix
    ["11AA2222233/4", "11AA2222233"],
    ["11A12222233/4", "11A12222233"],
    ["11992222233/4", "11992222233"],
    ["111X2222233/4", "111X2222233"],

    ["11AA2222233/41", "11AA2222233"],
    ["11A12222233/41", "11A12222233"],
    ["11992222233/41", "11992222233"],
    ["111X2222233/41", "111X2222233"],

    //  can have M or (M) suffix
    ["11AA2222233M", "11AA2222233"],
    ["11A12222233M", "11A12222233"],
    ["11992222233M", "11992222233"],
    ["111X2222233M", "111X2222233"],

    ["11AA2222233(M)", "11AA2222233"],
    ["11A12222233(M)", "11A12222233"],
    ["11992222233(M)", "11992222233"],
    ["111X2222233(M)", "111X2222233"],

    //  can have S or (S) suffix
    ["11AA2222233S", "11AA2222233"],
    ["11A12222233S", "11A12222233"],
    ["11992222233S", "11992222233"],
    ["111X2222233S", "111X2222233"],

    ["11AA2222233(S)", "11AA2222233"],
    ["11A12222233(S)", "11A12222233"],
    ["11992222233(S)", "11992222233"],
    ["111X2222233(S)", "111X2222233"],

    // can have combinations of M/S suffixes and numeric suffixes
    ["11AA2222233M/1", "11AA2222233"],
    ["11A12222233M/1", "11A12222233"],
    ["11992222233M/1", "11992222233"],
    ["111X2222233M/1", "111X2222233"],

    ["11AA2222233(M)/1", "11AA2222233"],
    ["11A12222233(M)/1", "11A12222233"],
    ["11992222233(M)/1", "11992222233"],
    ["111X2222233(M)/1", "111X2222233"],

    ["11AA2222233S/1", "11AA2222233"],
    ["11A12222233S/1", "11A12222233"],
    ["11992222233S/1", "11992222233"],
    ["111X2222233S/1", "111X2222233"],

    ["11AA2222233(S)/1", "11AA2222233"],
    ["11A12222233(S)/1", "11A12222233"],
    ["11992222233(S)/1", "11992222233"],
    ["111X2222233(S)/1", "111X2222233"],
  ])("will pass for correctly formatted values", (urn, rootUrn) => {
    expect(validateUrn(urn)).toEqual({ isValid: true, rootUrn });
  });

  it("will not pass for empty", () => {
    expect(validateUrn("")).toEqual({ isValid: false, rootUrn: null });
  });

  it("will not pass for whitespace", () => {
    expect(validateUrn(" ")).toEqual({ isValid: false, rootUrn: null });
    expect(validateUrn("  ")).toEqual({ isValid: false, rootUrn: null });
  });

  it.each([
    "111AA2222233",
    "11AAA22222333",
    "11AA22A2233",
    "13WD1234520N",
    "13WD1234520(N)",
    "13WD1234520NN",
    "13WD1234520(NN)",
    "11AA2222233/P",
    "13WD1234520/222",
    "13WD1234520M/222",
    "13WD1234520(M)/222",
  ])("will not pass for incorrectly formatted values", (urn) => {
    expect(validateUrn(urn)).toEqual({ isValid: false, rootUrn: null });
  });
});
