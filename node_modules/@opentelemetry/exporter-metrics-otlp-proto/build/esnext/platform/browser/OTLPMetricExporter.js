/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
import { OTLPMetricExporterBase } from '@opentelemetry/exporter-metrics-otlp-http';
import { MetricsExporterMetricsHelper, ProtobufMetricsSerializer, } from '@opentelemetry/otlp-transformer';
import { createLegacyOtlpBrowserExportDelegate, createLegacyOtlpBrowserExporterMetrics, } from '@opentelemetry/otlp-exporter-base/browser-http';
import { OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER } from '../../semconv';
export class OTLPMetricExporter extends OTLPMetricExporterBase {
    _url;
    constructor(config = {}) {
        super(createLegacyOtlpBrowserExportDelegate(config, ProtobufMetricsSerializer, OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, MetricsExporterMetricsHelper, config?.selfObsMeterProvider, 'v1/metrics', { 'Content-Type': 'application/x-protobuf' }), config);
        this._url = config.url;
    }
    /**
     * Sets the meter provider to use to collect metrics for the exporter itself.
     * @experimental This method is experimental and is subject to breaking changes in minor releases.
     */
    setSelfObsMeterProvider(meterProvider) {
        this.setMetrics(createLegacyOtlpBrowserExporterMetrics(OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, MetricsExporterMetricsHelper, this._url, meterProvider));
    }
}
//# sourceMappingURL=OTLPMetricExporter.js.map