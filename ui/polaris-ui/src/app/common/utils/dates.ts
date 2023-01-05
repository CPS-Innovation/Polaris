import { format, parseISO, differenceInYears } from "date-fns";

export const CommonDateTimeFormats = {
  ShortDate: "dd/MM/yyyy",
  ShortDateTextMonth: "dd MMM yyyy",
  ShortDateFullTextMonth: "dd LLLL yyyy",
};

export const formatDate = (isoDateString: string, dateTimeFormat: string) =>
  isoDateString && format(parseISO(isoDateString), dateTimeFormat);

export const getAgeFromIsoDate = (isoDateString: string) =>
  isoDateString && differenceInYears(new Date(), parseISO(isoDateString));
