import { format, parseISO, differenceInYears, isValid } from "date-fns";

export const CommonDateTimeFormats = {
  ShortDate: "dd/MM/yyyy",
  ShortDateTextMonth: "dd MMM yyyy",
  ShortDateFullTextMonth: "dd LLLL yyyy",
  ShortTime: "HH:mm",
};

export const formatDate = (isoDateString: string, dateTimeFormat: string) =>
  isoDateString && format(parseISO(isoDateString), dateTimeFormat);

export const formatTime = (isoDateString: string) => {
  try {
    return isoDateString.includes("T") && isValid(parseISO(isoDateString))
      ? formatDate(isoDateString, CommonDateTimeFormats.ShortTime)
      : null;
  } catch {
    // fallback to null if parseISO fails to parse the datetime
    return null;
  }
};

export const getAgeFromIsoDate = (isoDateString: string) =>
  isoDateString && differenceInYears(new Date(), parseISO(isoDateString));
