"use strict";
/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.METRIC_DB_CLIENT_CONNECTION_PENDING_REQUESTS = exports.METRIC_DB_CLIENT_CONNECTION_COUNT = exports.DB_SYSTEM_NAME_VALUE_POSTGRESQL = exports.DB_CLIENT_CONNECTION_STATE_VALUE_USED = exports.DB_CLIENT_CONNECTION_STATE_VALUE_IDLE = exports.ATTR_DB_CLIENT_CONNECTION_STATE = exports.ATTR_DB_CLIENT_CONNECTION_POOL_NAME = void 0;
/*
 * This file contains a copy of unstable semantic convention definitions
 * used by this package.
 * @see https://github.com/open-telemetry/opentelemetry-js/tree/main/semantic-conventions#unstable-semconv
 */
/**
 * The name of the connection pool; unique within the instrumented application. In case the connection pool implementation doesn't provide a name, instrumentation **SHOULD** use a combination of parameters that would make the name unique, for example, combining attributes `server.address`, `server.port`, and `db.namespace`, formatted as `server.address:server.port/db.namespace`. Instrumentations that generate connection pool name following different patterns **SHOULD** document it.
 *
 * @example myDataSource
 *
 * @experimental This attribute is experimental and is subject to breaking changes in minor releases of `@opentelemetry/semantic-conventions`.
 */
exports.ATTR_DB_CLIENT_CONNECTION_POOL_NAME = 'db.client.connection.pool.name';
/**
 * The state of a connection in the pool
 *
 * @example idle
 *
 * @experimental This attribute is experimental and is subject to breaking changes in minor releases of `@opentelemetry/semantic-conventions`.
 */
exports.ATTR_DB_CLIENT_CONNECTION_STATE = 'db.client.connection.state';
/**
 * Enum value "idle" for attribute {@link ATTR_DB_CLIENT_CONNECTION_STATE}.
 */
exports.DB_CLIENT_CONNECTION_STATE_VALUE_IDLE = 'idle';
/**
 * Enum value "used" for attribute {@link ATTR_DB_CLIENT_CONNECTION_STATE}.
 */
exports.DB_CLIENT_CONNECTION_STATE_VALUE_USED = 'used';
/**
 * Enum value "postgresql" for attribute {@link ATTR_DB_SYSTEM_NAME}.
 *
 * [PostgreSQL](https://www.postgresql.org/)
 */
exports.DB_SYSTEM_NAME_VALUE_POSTGRESQL = 'postgresql';
/**
 * The number of connections that are currently in state described by the `state` attribute
 *
 * @experimental This metric is experimental and is subject to breaking changes in minor releases of `@opentelemetry/semantic-conventions`.
 */
exports.METRIC_DB_CLIENT_CONNECTION_COUNT = 'db.client.connection.count';
/**
 * The number of current pending requests for an open connection
 *
 * @experimental This metric is experimental and is subject to breaking changes in minor releases of `@opentelemetry/semantic-conventions`.
 */
exports.METRIC_DB_CLIENT_CONNECTION_PENDING_REQUESTS = 'db.client.connection.pending_requests';
//# sourceMappingURL=semconv.js.map