import { CmsAuthError } from "../../../errors/CmsAuthError";
export declare const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";
export declare const FAIL_CORRELATION_ID_QUERY_PARAM = "fail-correlation-id";
export declare const AUTH_FAIL_REASON_QUERY_PARAM = "auth-fail-reason";
export declare const CORRELATION_ID = "Correlation-Id";
export type FetchArgs = Parameters<typeof fetch>;
type ReauthMode = "in-situ" | "full-window";
export declare const PREFERRED_AUTH_MODE: ReauthMode;
export declare const STATUS_CODES: {
    UNAUTHORIZED: 401;
    FORBIDDEN_STATUS_CODE: 403;
    GONE_STATUS_CODE: 410;
    UNAVAILABLE_FOR_LEGAL_REASONS_STATUS_CODE: 451;
};
export declare enum AuthFailReason {
    NoCookies = "no-cookies",
    NoCmsAuthCookie = "no-cmsauth-cookie",
    CmsAuthNotValid = "cms-auth-not-valid",
    CmsModernAuthNotValid = "cms-modern-auth-not-valid",
    InSituAttemptFailed = "in-situ-attempt-failed",
    UnexpectedError = "unexpected-error"
}
export declare const buildCmsAuthError: (authFailReason: AuthFailReason | null | undefined) => CmsAuthError;
export declare const isCmsAuthFail: (response: Response) => boolean;
export declare const getCorrelationIdFromFetchArgs: (input: RequestInfo | URL, init?: RequestInit) => string;
type AssembleRedirectUrlArg = {
    outboundUrl: string;
    inboundUrl: string;
    terminationUrl: string;
    correlationId: string | null;
};
export declare const assembleRedirectUrl: ({ outboundUrl, inboundUrl, terminationUrl, correlationId }: AssembleRedirectUrlArg) => string;
export {};
