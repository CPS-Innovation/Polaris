import { getAccessToken } from "../../../../auth";
import { GATEWAY_SCOPE, REDACTION_LOG_SCOPE } from "../../../../config";
import { generateGuid } from "./../generate-guid";
import { CORRELATION_ID } from "./core";

export const correlationId = (existingCorrelationId?: string) => ({
  [CORRELATION_ID]: existingCorrelationId || generateGuid(),
});

export const auth = async () => ({
  Authorization: `Bearer ${
    GATEWAY_SCOPE ? await getAccessToken([GATEWAY_SCOPE]) : "TEST"
  }`,
});

export const authRedactionLog = async () => ({
  Authorization: `Bearer ${
    REDACTION_LOG_SCOPE ? await getAccessToken([REDACTION_LOG_SCOPE]) : "TEST"
  }`,
});

export const buildHeaders = async (
  knownCorrelationId?: string
): Promise<Record<string, string>> => {
  return {
    ...correlationId(knownCorrelationId || undefined),
    ...(await auth()),
  };
};

export const buildHeadersRedactionLog = async (): Promise<
  Record<string, string>
> => {
  return {
    ...correlationId(undefined),
    ...(await authRedactionLog()),
  };
};
