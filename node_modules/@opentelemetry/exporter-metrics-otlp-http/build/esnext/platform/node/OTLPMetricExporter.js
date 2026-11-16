/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
import { OTLPMetricExporterBase } from '../../OTLPMetricExporterBase';
import { JsonMetricsSerializer, MetricsExporterMetricsHelper, } from '@opentelemetry/otlp-transformer';
import { convertLegacyHttpOptions, createOtlpHttpExportDelegate, createOtlpHttpExporterMetrics, } from '@opentelemetry/otlp-exporter-base/node-http';
import { OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER } from '../../semconv';
/**
 * OTLP Metric Exporter for Node.js
 */
export class OTLPMetricExporter extends OTLPMetricExporterBase {
    _url;
    constructor(config) {
        super(createOtlpHttpExportDelegate(convertLegacyHttpOptions(config ?? {}, 'METRICS', 'v1/metrics', {
            'Content-Type': 'application/json',
        }), JsonMetricsSerializer, OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, MetricsExporterMetricsHelper, config?.selfObsMeterProvider), config);
        this._url = config?.url;
    }
    /**
     * Sets the meter provider to use to collect metrics for the exporter itself.
     * @experimental This method is experimental and is subject to breaking changes in minor releases.
     */
    setSelfObsMeterProvider(meterProvider) {
        this.setMetrics(createOtlpHttpExporterMetrics(OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, MetricsExporterMetricsHelper, this._url, meterProvider));
    }
}
//# sourceMappingURL=OTLPMetricExporter.js.map