"use strict";
/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.AsyncLocalStorageContextManager = void 0;
const api_1 = require("@opentelemetry/api");
const async_hooks_1 = require("async_hooks");
const AbstractAsyncHooksContextManager_1 = require("./AbstractAsyncHooksContextManager");
/**
 * Wrapper around a token and _asyncLocalStorage to mirror the behavior of
 * a Node.js RunScope
 *
 * @internal not intended for direct public consumption. Will be removed once
 * withScope is available on all supported Node.js versions
 */
class DisposeOnceToken {
    _isDisposed = false;
    _previousContext;
    _asyncLocalStorage;
    constructor(previousContext, asyncLocalStorage) {
        this._previousContext = previousContext;
        this._asyncLocalStorage = asyncLocalStorage;
    }
    dispose() {
        if (this._isDisposed) {
            return;
        }
        this._asyncLocalStorage.enterWith(this._previousContext);
        this._isDisposed = true;
    }
}
class AsyncLocalStorageContextManager extends AbstractAsyncHooksContextManager_1.AbstractAsyncHooksContextManager {
    _asyncLocalStorage;
    constructor() {
        super();
        this._asyncLocalStorage = new async_hooks_1.AsyncLocalStorage();
    }
    active() {
        return this._asyncLocalStorage.getStore() ?? api_1.ROOT_CONTEXT;
    }
    with(context, fn, thisArg, ...args) {
        const cb = thisArg == null ? fn : fn.bind(thisArg);
        return this._asyncLocalStorage.run(context, cb, ...args);
    }
    enable() {
        return this;
    }
    disable() {
        this._asyncLocalStorage.disable();
        return this;
    }
    /**
     * Imperatively sets `context` as active for the current async execution chain
     * and operations spawned from it. Returns a {@link ContextManagementToken} whose `dispose()`
     * restores the previous context (see {@link ContextManager.attach}).
     *
     * On Node.js 25.9+, delegates to `AsyncLocalStorage.withScope()` which returns
     * a native `RunScope`. On older Node.js versions, falls back to `enterWith()` with
     * a manual token.
     *
     * **Caveat for async functions:** Both `withScope()` and `enterWith()` affect the
     * entire current async execution chain. If `attach()` is called inside an async
     * function before the first `await`, the context change will leak into the caller's
     * context and remain active there until something else restores it. Prefer `with()`
     * for async code.
     *
     * @experimental This API is experimental and may change in minor releases without prior notice.
     */
    attach(context) {
        // Node.js 25.9+: withScope() returns a RunScope with dispose()
        const withScope = this._asyncLocalStorage.withScope;
        if (withScope) {
            return withScope.call(this._asyncLocalStorage, context);
        }
        // Fallback for older Node.js - this can be dropped when the minimum supported
        // Node.js version of this package is 25.9 or higher.
        const previousContext = this.active();
        this._asyncLocalStorage.enterWith(context);
        return new DisposeOnceToken(previousContext, this._asyncLocalStorage);
    }
}
exports.AsyncLocalStorageContextManager = AsyncLocalStorageContextManager;
//# sourceMappingURL=AsyncLocalStorageContextManager.js.map