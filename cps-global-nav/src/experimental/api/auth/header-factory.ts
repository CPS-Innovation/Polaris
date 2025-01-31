import { getAccessToken } from "../../ad-auth/get-access-token";
import { CORRELATION_ID, GATEWAY_SCOPE } from "../../config";
import { generateGuid } from "../../generate-guid";

export const correlationId = (existingCorrelationId?: string) => ({
  [CORRELATION_ID]: existingCorrelationId || generateGuid(),
});

export const auth = async () => ({
  Authorization: `Bearer ${GATEWAY_SCOPE ? await getAccessToken([GATEWAY_SCOPE]) : "TEST"}`,
});

export const buildHeaders = async (knownCorrelationId?: string): Promise<Record<string, string>> => {
  return {
    ...correlationId(knownCorrelationId || undefined),
    ...(await auth()),
  };
};
