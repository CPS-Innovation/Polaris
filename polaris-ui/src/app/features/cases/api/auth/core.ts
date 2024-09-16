import { CmsAuthError } from "../../../../common/errors/CmsAuthError";
import { REAUTH_REDIRECT_URL } from "../../../../config";

export const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";
export const FAIL_CORRELATION_ID_QUERY_PARAM = "fail-correlation-id";
export const AUTH_FAIL_REASON_QUERY_PARAM = "auth-fail-reason";
export const CORRELATION_ID = "Correlation-Id";

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
        "It may be the case that you are not yet logged in to CMS, or that CMS has recently logged you out.";
      break;
    case AuthFailReason.CmsAuthNotValid:
    case AuthFailReason.CmsModernAuthNotValid:
      customMessage =
        "It may be the case that your CMS session has expired, or you may have logged in to CMS on another computer since ";
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

export const buildRedirectUrl = (
  terminationUrl: string,
  correlationId: string | null | undefined
) => {
  const delimiter = window.location.href.includes("?") ? "&" : "?";
  let nextUrl = `${encodeURIComponent(
    terminationUrl + delimiter + REAUTHENTICATION_INDICATOR_QUERY_PARAM
  )}`;

  if (correlationId) {
    nextUrl += `&${FAIL_CORRELATION_ID_QUERY_PARAM}=${correlationId}`;
  }

  return `${REAUTH_REDIRECT_URL}${encodeURIComponent(nextUrl)}`;
};

export const isCmsAuthFail = (response: Response) =>
  response.status === STATUS_CODES.UNAUTHORIZED;

export const getCorrelationIdFromFetchArgs = (
  ...args: Parameters<typeof fetch>
) => {
  const headers = args[1]?.headers as Record<string, string> | undefined;
  return headers?.[CORRELATION_ID];
};
