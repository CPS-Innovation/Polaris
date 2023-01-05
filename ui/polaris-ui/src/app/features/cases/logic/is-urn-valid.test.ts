import { isUrnValid } from "./is-urn-valid";

describe("urnRegex", () => {
  it("will pass for correctly formatted values", () => {
    // dd/aa/ddddddd - traditional
    expect(isUrnValid("11AA2222233")).toBe(true);

    // dd/ad/ddddddd - commonly seen
    expect(isUrnValid("11Z12222233")).toBe(true);

    // dd/dd/ddddddd - seen in e.g. NCA cases
    expect(isUrnValid("11992222233")).toBe(true);

    // dd/da/ddddddd - seen in data dump or URNs from Jake, may or may not be a typo in data
    //  but we will allow it at the moment
    expect(isUrnValid("111X2222233")).toBe(true);

    // now test the suffix variants
    //  can have /1 or /11 suffix
    expect(isUrnValid("11AA2222233/4")).toBe(true);
    expect(isUrnValid("11A12222233/4")).toBe(true);
    expect(isUrnValid("11992222233/4")).toBe(true);
    expect(isUrnValid("111X2222233/4")).toBe(true);

    expect(isUrnValid("11AA2222233/41")).toBe(true);
    expect(isUrnValid("11A12222233/41")).toBe(true);
    expect(isUrnValid("11992222233/41")).toBe(true);
    expect(isUrnValid("111X2222233/41")).toBe(true);

    //  can have M or (M) suffix
    expect(isUrnValid("11AA2222233M")).toBe(true);
    expect(isUrnValid("11A12222233M")).toBe(true);
    expect(isUrnValid("11992222233M")).toBe(true);
    expect(isUrnValid("111X2222233M")).toBe(true);

    expect(isUrnValid("11AA2222233(M)")).toBe(true);
    expect(isUrnValid("11A12222233(M)")).toBe(true);
    expect(isUrnValid("11992222233(M)")).toBe(true);
    expect(isUrnValid("111X2222233(M)")).toBe(true);

    //  can have S or (S) suffix
    expect(isUrnValid("11AA2222233S")).toBe(true);
    expect(isUrnValid("11A12222233S")).toBe(true);
    expect(isUrnValid("11992222233S")).toBe(true);
    expect(isUrnValid("111X2222233S")).toBe(true);

    expect(isUrnValid("11AA2222233(S)")).toBe(true);
    expect(isUrnValid("11A12222233(S)")).toBe(true);
    expect(isUrnValid("11992222233(S)")).toBe(true);
    expect(isUrnValid("111X2222233(S)")).toBe(true);

    // can have combinations of M/S suffixes and numeric suffixes
    expect(isUrnValid("11AA2222233M/1")).toBe(true);
    expect(isUrnValid("11A12222233M/1")).toBe(true);
    expect(isUrnValid("11992222233M/1")).toBe(true);
    expect(isUrnValid("111X2222233M/1")).toBe(true);

    expect(isUrnValid("11AA2222233(M)/1")).toBe(true);
    expect(isUrnValid("11A12222233(M)/1")).toBe(true);
    expect(isUrnValid("11992222233(M)/1")).toBe(true);
    expect(isUrnValid("111X2222233(M)/1")).toBe(true);

    expect(isUrnValid("11AA2222233S/1")).toBe(true);
    expect(isUrnValid("11A12222233S/1")).toBe(true);
    expect(isUrnValid("11992222233S/1")).toBe(true);
    expect(isUrnValid("111X2222233S/1")).toBe(true);

    expect(isUrnValid("11AA2222233(S)/1")).toBe(true);
    expect(isUrnValid("11A12222233(S)/1")).toBe(true);
    expect(isUrnValid("11992222233(S)/1")).toBe(true);
    expect(isUrnValid("111X2222233(S)/1")).toBe(true);
  });

  it("will not pass for empty", () => {
    expect(isUrnValid("")).toBe(false);
  });

  it("will not pass for whitespace", () => {
    expect(isUrnValid(" ")).toBe(false);
    expect(isUrnValid("  ")).toBe(false);
  });

  it("will not pass for incorrectly formatted values", () => {
    expect(isUrnValid("111AA2222233")).toBe(false);
    expect(isUrnValid("11AAA22222333")).toBe(false);
    expect(isUrnValid("11AA22A2233")).toBe(false);
    expect(isUrnValid("13WD1234520N")).toBe(false);
    expect(isUrnValid("13WD1234520(N)")).toBe(false);
    expect(isUrnValid("13WD1234520NN")).toBe(false);
    expect(isUrnValid("13WD1234520(NN)")).toBe(false);
    expect(isUrnValid("11AA2222233/P")).toBe(false);
    expect(isUrnValid("13WD1234520/222")).toBe(false);
    expect(isUrnValid("13WD1234520M/222")).toBe(false);
    expect(isUrnValid("13WD1234520(M)/222")).toBe(false);
  });
});
