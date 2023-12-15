import { formatDate, CommonDateTimeFormats, formatTime } from "./dates";

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

  it('should return null if the input does not include "T"', () => {
    expect(formatTime("2022-01-01")).toBeNull();
  });

  it("should return null if the input is not a valid ISO date string", () => {
    expect(formatTime("not a date")).toBeNull();
  });

  it("should return the formatted time if the input is a valid ISO date string", () => {
    expect(formatTime("2022-01-01T13:45:00Z")).toBe("13:45");
  });

  it("should return the formatted time if the input is a valid ISO date BST string", () => {
    expect(formatTime("2022-08-08T13:45:00Z")).toBe("14:45");
  });
});
