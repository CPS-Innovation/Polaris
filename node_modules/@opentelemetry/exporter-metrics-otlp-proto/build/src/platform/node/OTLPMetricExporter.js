"use strict";
/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.OTLPMetricExporter = void 0;
const exporter_metrics_otlp_http_1 = require("@opentelemetry/exporter-metrics-otlp-http");
const otlp_transformer_1 = require("@opentelemetry/otlp-transformer");
const node_http_1 = require("@opentelemetry/otlp-exporter-base/node-http");
const semconv_1 = require("../../semconv");
class OTLPMetricExporter extends exporter_metrics_otlp_http_1.OTLPMetricExporterBase {
    _url;
    constructor(config) {
        super((0, node_http_1.createOtlpHttpExportDelegate)((0, node_http_1.convertLegacyHttpOptions)(config ?? {}, 'METRICS', 'v1/metrics', {
            'Content-Type': 'application/x-protobuf',
        }), otlp_transformer_1.ProtobufMetricsSerializer, semconv_1.OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, otlp_transformer_1.MetricsExporterMetricsHelper, config?.selfObsMeterProvider), config);
        this._url = config?.url;
    }
    /**
     * Sets the meter provider to use to collect metrics for the exporter itself.
     * @experimental This method is experimental and is subject to breaking changes in minor releases.
     */
    setSelfObsMeterProvider(meterProvider) {
        this.setMetrics((0, node_http_1.createOtlpHttpExporterMetrics)(semconv_1.OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, otlp_transformer_1.MetricsExporterMetricsHelper, this._url, meterProvider));
    }
}
exports.OTLPMetricExporter = OTLPMetricExporter;
//# sourceMappingURL=OTLPMetricExporter.js.map