/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
export { WebTracerProvider } from './WebTracerProvider';
export { StackContextManager } from './StackContextManager';
export { PerformanceTimingNames } from './enums/PerformanceTimingNames';
export { addSpanNetworkEvent, addSpanNetworkEvents, getElementXPath, getResource, hasKey, normalizeUrl, parseUrl, shouldPropagateTraceHeaders, sortResources, } from './utils';
export { AlwaysOffSampler, AlwaysOnSampler, BasicTracerProvider, BatchSpanProcessor, ConsoleSpanExporter, InMemorySpanExporter, NoopSpanProcessor, ParentBasedSampler, RandomIdGenerator, SamplingDecision, SimpleSpanProcessor, TraceIdRatioBasedSampler, } from '@opentelemetry/sdk-trace-base';
//# sourceMappingURL=index.js.map