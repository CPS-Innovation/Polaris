import type { Context } from '@opentelemetry/api';
import type { ContextManagementToken } from './types';
import { AbstractAsyncHooksContextManager } from './AbstractAsyncHooksContextManager';
export declare class AsyncLocalStorageContextManager extends AbstractAsyncHooksContextManager {
    private _asyncLocalStorage;
    constructor();
    active(): Context;
    with<A extends unknown[], F extends (...args: A) => ReturnType<F>>(context: Context, fn: F, thisArg?: ThisParameterType<F>, ...args: A): ReturnType<F>;
    enable(): this;
    disable(): this;
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
    attach(context: Context): ContextManagementToken;
}
//# sourceMappingURL=AsyncLocalStorageContextManager.d.ts.map