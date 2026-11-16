import type { ResourceDetectionConfig } from '../../../config';
import type { DetectedResource, ResourceDetector } from '../../../types';
/**
 * ServiceInstanceIdDetector detects the resources related to the service instance ID.
 */
declare class ServiceInstanceIdDetector implements ResourceDetector {
    private _serviceInstanceId;
    detect(_config?: ResourceDetectionConfig): DetectedResource;
}
/**
 * @experimental
 */
export declare const serviceInstanceIdDetector: ServiceInstanceIdDetector;
export {};
//# sourceMappingURL=ServiceInstanceIdDetector.d.ts.map