import { type MeterProvider } from '@opentelemetry/api';
import type { OTLPMetricExporterOptions } from '@opentelemetry/exporter-metrics-otlp-http';
import { OTLPMetricExporterBase } from '@opentelemetry/exporter-metrics-otlp-http';
import type { OTLPExporterNodeConfigBase } from '@opentelemetry/otlp-exporter-base';
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