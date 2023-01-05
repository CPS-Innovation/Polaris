import { getAccessToken } from "../../../auth";
import { GATEWAY_SCOPE } from "../../../config";
import { v4 as uuidv4 } from "uuid";

const CORRELATION_ID = "Correlation-Id";

export const correlationId = (existingCorrelationId?: string) => ({
  [CORRELATION_ID]: existingCorrelationId || uuidv4(),
});

export const auth = async () => ({
  Authorization: `Bearer ${
    GATEWAY_SCOPE ? await getAccessToken([GATEWAY_SCOPE]) : "TEST"
  }`,
});

export const upstreamHeader = async () => ({
  "Upstream-Token": "not-implemented-yet",
});
