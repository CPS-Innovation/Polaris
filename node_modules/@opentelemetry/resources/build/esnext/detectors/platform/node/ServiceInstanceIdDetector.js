/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
import { ATTR_SERVICE_INSTANCE_ID } from '../../../semconv';
import { randomUUID } from 'crypto';
/**
 * ServiceInstanceIdDetector detects the resources related to the service instance ID.
 */
class ServiceInstanceIdDetector {
    // Multiple calls to ServiceInstanceIdDetector return the same ID.
    _serviceInstanceId;
    detect(_config) {
        if (!this._serviceInstanceId) {
            this._serviceInstanceId = randomUUID();
        }
        return {
            attributes: {
                [ATTR_SERVICE_INSTANCE_ID]: this._serviceInstanceId,
            },
        };
    }
}
/**
 * @experimental
 */
export const serviceInstanceIdDetector = new ServiceInstanceIdDetector();
//# sourceMappingURL=ServiceInstanceIdDetector.js.map