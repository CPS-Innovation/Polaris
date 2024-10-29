export const LOCKED_STATUS_CODE = 423;

export const isNewTime = (incomingIsoTime: string, existingIsoTime: string) =>
  incomingIsoTime > existingIsoTime;
