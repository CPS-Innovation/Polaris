/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
import { BasicTracerProvider } from '@opentelemetry/sdk-trace-base';
import { StackContextManager } from './StackContextManager';
import { trace, context, propagation } from '@opentelemetry/api';
import { CompositePropagator, W3CBaggagePropagator, W3CTraceContextPropagator, } from '@opentelemetry/core';
function setupContextManager(contextManager) {
    // null means 'do not register'
    if (contextManager === null) {
        return;
    }
    // undefined means 'register default'
    if (contextManager === undefined) {
        const defaultContextManager = new StackContextManager();
        defaultContextManager.enable();
        context.setGlobalContextManager(defaultContextManager);
        return;
    }
    contextManager.enable();
    context.setGlobalContextManager(contextManager);
}
function setupPropagator(propagator) {
    // null means 'do not register'
    if (propagator === null) {
        return;
    }
    // undefined means 'register default'
    if (propagator === undefined) {
        propagation.setGlobalPropagator(new CompositePropagator({
            propagators: [
                new W3CTraceContextPropagator(),
                new W3CBaggagePropagator(),
            ],
        }));
        return;
    }
    propagation.setGlobalPropagator(propagator);
}
/**
 * This class represents a web tracer with {@link StackContextManager}
 */
export class WebTracerProvider extends BasicTracerProvider {
    /**
     * Constructs a new Tracer instance.
     * @param config Web Tracer config
     */
    constructor(config = {}) {
        super(config);
    }
    /**
     * Register this TracerProvider for use with the OpenTelemetry API.
     * Undefined values may be replaced with defaults, and
     * null values will be skipped.
     *
     * @param config Configuration object for SDK registration
     */
    register(config = {}) {
        trace.setGlobalTracerProvider(this);
        setupPropagator(config.propagator);
        setupContextManager(config.contextManager);
    }
}
//# sourceMappingURL=WebTracerProvider.js.map