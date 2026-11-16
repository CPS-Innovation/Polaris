import { type MeterProvider } from '@opentelemetry/api';
import type { OTLPMetricExporterOptions } from '../../OTLPMetricExporterOptions';
import { OTLPMetricExporterBase } from '../../OTLPMetricExporterBase';
import type { OTLPExporterNodeConfigBase } from '@opentelemetry/otlp-exporter-base';
/**
 * OTLP Metric Exporter for Node.js
 */
export declare class OTLPMetricExporter extends OTLPMetricExporterBase {
    private readonly _url;
    constructor(config?: OTLPExporterNodeConfigBase & OTLPMetricExporterOptions);
    /**
     * Sets the meter provider to use to collect metrics for the exporter itself.
     * @experimental This method is experimental and is subject to breaking changes in minor releases.
     */
    setSelfObsMeterProvider(meterProvider: MeterProvider): void;
}
//# sourceMappingURL=OTLPMetricExporter.d.ts.map