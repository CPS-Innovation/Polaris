/// <reference types="node" />
import type { Attributes, Span, DiagLogger } from '@opentelemetry/api';
import { SpanStatusCode, SpanKind } from '@opentelemetry/api';
import type { IncomingHttpHeaders, IncomingMessage, OutgoingHttpHeaders, RequestOptions, ServerResponse } from 'http';
import * as url from 'url';
import type { Err, IgnoreMatcher, ParsedRequestOptions } from './internal-types';
/**
 * Get an absolute url
 */
export declare const getAbsoluteUrl: (requestUrl: ParsedRequestOptions | null, headers: IncomingHttpHeaders | OutgoingHttpHeaders, fallbackProtocol?: string, redactedQueryParams?: string[]) => string;
/**
 * Parse status code from HTTP response. [More details](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/data-http.md#status)
 */
export declare const parseResponseStatus: (kind: SpanKind, statusCode?: number) => SpanStatusCode;
/**
 * Check whether the given obj match pattern
 * @param constant e.g URL of request
 * @param pattern Match pattern
 */
export declare const satisfiesPattern: (constant: string, pattern: IgnoreMatcher) => boolean;
/**
 * Sets the span with the error passed in params
 * @param {Span} span the span that need to be set
 * @param {Error} error error that will be set to span
 */
export declare const setSpanWithError: (span: Span, error: Err) => void;
export declare const isCompressed: (headers: OutgoingHttpHeaders | IncomingHttpHeaders) => boolean;
/**
 * Makes sure options is an url object
 * return an object with default value and parsed options
 * @param logger component logger
 * @param options original options for the request
 * @param [extraOptions] additional options for the request
 */
export declare const getRequestInfo: (logger: DiagLogger, options: url.URL | RequestOptions | string, extraOptions?: RequestOptions) => {
    origin: string;
    pathname: string;
    method: string;
    invalidUrl: boolean;
    optionsParsed: RequestOptions;
};
/**
 * Makes sure options is of type string or object
 * @param options for the request
 */
export declare const isValidOptionsType: (options: unknown) => boolean;
export declare const extractHostnameAndPort: (requestOptions: Pick<ParsedRequestOptions, 'hostname' | 'host' | 'port' | 'protocol'>) => {
    hostname: string;
    port: number | string;
};
/**
 * Returns outgoing request attributes scoped to the options passed to the request
 * @param {ParsedRequestOptions} requestOptions the same options used to make the request
 * @param {{ component: string, hostname: string, hookAttributes?: Attributes }} options used to pass data needed to create attributes
 */
export declare const getOutgoingRequestAttributes: (requestOptions: ParsedRequestOptions, options: {
    component: string;
    hostname: string;
    port: string | number;
    hookAttributes?: Attributes;
    redactedQueryParams?: string[];
}, enableSyntheticSourceDetection: boolean) => Attributes;
/**
 * Returns outgoing request attributes scoped to the response data
 * @param {IncomingMessage} response the response object
 */
export declare const getOutgoingRequestAttributesOnResponse: (response: IncomingMessage) => Attributes;
/**
 * Returns outgoing request Metric attributes scoped to the response data
 * @param {Attributes} spanAttributes the span attributes
 */
export declare const getOutgoingStableRequestMetricAttributesOnResponse: (spanAttributes: Attributes) => Attributes;
/**
 * Get server.address and port according to http semconv 1.27
 * https://github.com/open-telemetry/semantic-conventions/blob/bf0a2c1134f206f034408b201dbec37960ed60ec/docs/http/http-spans.md#setting-serveraddress-and-serverport-attributes
 */
export declare function getRemoteClientAddress(request: IncomingMessage): string | null;
/**
 * Returns incoming request attributes scoped to the request data
 * @param {IncomingMessage} request the request object
 * @param {{ component: string, hookAttributes?: Attributes, enableSyntheticSourceDetection: boolean }} options used to pass data needed to create attributes
 */
export declare const getIncomingRequestAttributes: (request: IncomingMessage, options: {
    component: 'http' | 'https';
    hookAttributes?: Attributes;
    enableSyntheticSourceDetection: boolean;
}, logger: DiagLogger) => Attributes;
/**
 * Returns incoming request attributes scoped to the response data
 * @param {(ServerResponse & { socket: Socket; })} response the response object
 */
export declare const getIncomingRequestAttributesOnResponse: (response: ServerResponse) => Attributes;
/**
 * Returns incoming stable request Metric attributes scoped to the request data
 * @param {Attributes} spanAttributes the span attributes
 */
export declare const getIncomingStableRequestMetricAttributesOnResponse: (spanAttributes: Attributes) => Attributes;
export declare function headerCapture(type: 'request' | 'response', headers: string[]): (getHeader: (key: string) => undefined | string | string[] | number) => Attributes;
//# sourceMappingURL=utils.d.ts.map