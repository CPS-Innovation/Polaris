import { format, parseISO, differenceInYears, isValid } from "date-fns";
import { formatInTimeZone } from "date-fns-tz";

import enGB from "date-fns/locale/en-GB";

export const CommonDateTimeFormats = {
  ShortDate: "dd/MM/yyyy",
  ShortDateTextMonth: "dd MMM yyyy",
  ShortDateFullTextMonth: "dd LLLL yyyy",
  ShortTime: "HH:mm",
};

export const formatDate = (isoDateString: string, dateTimeFormat: string) =>
  isoDateString && format(parseISO(isoDateString), dateTimeFormat);

const formatToUkDateTime = (isoDateString: string) =>
  isoDateString &&
  formatInTimeZone(
    parseISO(isoDateString),
    "Europe/London",
    CommonDateTimeFormats.ShortTime,
    { locale: enGB }
  );

export const formatTime = (isoDateString: string) => {
  try {
    return isoDateString.includes("T") && isValid(parseISO(isoDateString))
      ? formatToUkDateTime(isoDateString)
      : null;
  } catch {
    // fallback to null if parseISO fails to parse the datetime
    return null;
  }
};

export const getAgeFromIsoDate = (isoDateString: string) =>
  isoDateString && differenceInYears(new Date(), parseISO(isoDateString));
