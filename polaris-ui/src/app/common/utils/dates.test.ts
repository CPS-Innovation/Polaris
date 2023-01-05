import { formatDate, CommonDateTimeFormats } from "./dates";

describe("formatDate", () => {
  it("can format a short iso date", () => {
    expect(formatDate("2021-02-01", CommonDateTimeFormats.ShortDate)).toBe(
      "01/02/2021"
    );
  });

  it("can format a long iso date", () => {
    expect(
      formatDate("2021-02-01T23:59:59", CommonDateTimeFormats.ShortDate)
    ).toBe("01/02/2021");
  });
});
