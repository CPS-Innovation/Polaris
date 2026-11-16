import { type MeterProvider } from '@opentelemetry/api';
import type { OTLPMetricExporterOptions } from '../../OTLPMetricExporterOptions';
import { OTLPMetricExporterBase } from '../../OTLPMetricExporterBase';
import type { OTLPExporterConfigBase } from '@opentelemetry/otlp-exporter-base';
/**
 * Collector Metric Exporter for Web
 */
export declare class OTLPMetricExporter extends OTLPMetricExporterBase {
    private readonly _url;
    constructor(config?: OTLPExporterConfigBase & OTLPMetricExporterOptions);
    /**
     * Sets the meter provider to use to collect metrics for the exporter itself.
     * @experimental This method is experimental and is subject to breaking changes in minor releases.
     */
    setSelfObsMeterProvider(meterProvider: MeterProvider): void;
}
//# sourceMappingURL=OTLPMetricExporter.d.ts.map