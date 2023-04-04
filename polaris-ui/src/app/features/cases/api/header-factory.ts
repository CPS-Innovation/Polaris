import { getAccessToken } from "../../../auth";
import { GATEWAY_SCOPE } from "../../../config";
import { generateGuid } from "./generate-guid";

const CORRELATION_ID = "Correlation-Id";

export const correlationId = (existingCorrelationId?: string) => ({
  [CORRELATION_ID]: existingCorrelationId || generateGuid(),
});

export const auth = async () => ({
  Authorization: `Bearer ${
    GATEWAY_SCOPE ? await getAccessToken([GATEWAY_SCOPE]) : "TEST"
  }`,
});
