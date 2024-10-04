import { CmsAuthError } from "../../../../common/errors/CmsAuthError";
import {
  REAUTH_REDIRECT_URL_OUTBOUND,
  REAUTH_REDIRECT_URL_OUTBOUND_E2E,
  REAUTH_REDIRECT_URL_INBOUND,
  REAUTH_USE_IN_SITU_REFRESH,
} from "../../../../config";

export const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";
export const FAIL_CORRELATION_ID_QUERY_PARAM = "fail-correlation-id";
export const AUTH_FAIL_REASON_QUERY_PARAM = "auth-fail-reason";
export const CORRELATION_ID = "Correlation-Id";

export const PREFERRED_AUTH_MODE = REAUTH_USE_IN_SITU_REFRESH
  ? "in-situ"
  : ("full-window" as const);

export const STATUS_CODES = {
  UNAUTHORIZED: 401 as const,
  FORBIDDEN_STATUS_CODE: 403 as const,
  GONE_STATUS_CODE: 410 as const,
  UNAVAILABLE_FOR_LEGAL_REASONS_STATUS_CODE: 451 as const,
};

export enum AuthFailReason {
  NoCookies = "no-cookies",
  NoCmsAuthCookie = "no-cmsauth-cookie",
  CmsAuthNotValid = "cms-auth-not-valid",
  CmsModernAuthNotValid = "cms-modern-auth-not-valid",
  InSituAttemptFailed = "in-situ-attempt-failed",
  UnexpectedError = "unexpected-error",
}

export const buildCmsAuthError = (
  authFailReason: AuthFailReason | null | undefined
) => {
  let customMessage: string;
  switch (authFailReason) {
    case AuthFailReason.NoCookies:
    case AuthFailReason.NoCmsAuthCookie:
      customMessage =
        "It may be the case that you are not yet logged in to CMS, or that the CMS tab that you have been using has recently logged you out.";
      break;
    case AuthFailReason.CmsAuthNotValid:
    case AuthFailReason.CmsModernAuthNotValid:
      customMessage =
        "It may be the case that your CMS session has expired, or you may have logged in to CMS on another computer since logging in to this one.";
      break;
    case AuthFailReason.InSituAttemptFailed:
      customMessage =
        "The attempt made by Casework App to reconnect to your CMS session did not succeed.";
      break;
    case AuthFailReason.UnexpectedError:
    default:
      customMessage =
        "An unexpected error occurred related to your current CMS log in session.";
  }

  return new CmsAuthError(
    `Unable to connect to CMS (${authFailReason}).`,
    customMessage || undefined
  );
};

export const isCmsAuthFail = (response: Response) =>
  response.status === STATUS_CODES.UNAUTHORIZED;

export const getCorrelationIdFromFetchArgs = (
  ...args: Parameters<typeof fetch>
) => {
  const headers = args[1]?.headers as Record<string, string> | undefined;
  return headers?.[CORRELATION_ID] || null;
};

type AssembleRedirectUrlArg = {
  outboundUrl: string;
  inboundUrl: string;
  terminationUrl: string;
  correlationId: string | null;
};
export const assembleRedirectUrl = ({
  outboundUrl,
  inboundUrl,
  terminationUrl,
  correlationId,
}: AssembleRedirectUrlArg) =>
  outboundUrl +
  "?r=" +
  encodeURIComponent(
    `${inboundUrl}?fail-correlation-id=${correlationId}&polaris-ui-url=${encodeURIComponent(
      terminationUrl
    )}`
  );
