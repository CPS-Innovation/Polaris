import { parseISO, isValid } from "date-fns";

export const shouldTriggerPipelineRefresh = (
  lastModifiedDateTime: string,
  localLastRefreshTime: string
): boolean => {
  if (
    !isValid(parseISO(lastModifiedDateTime)) ||
    !isValid(parseISO(localLastRefreshTime))
  )
    return true;
  return localLastRefreshTime < lastModifiedDateTime;
};
