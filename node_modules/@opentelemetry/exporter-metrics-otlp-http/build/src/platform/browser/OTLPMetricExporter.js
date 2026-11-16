"use strict";
/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.OTLPMetricExporter = void 0;
const OTLPMetricExporterBase_1 = require("../../OTLPMetricExporterBase");
const otlp_transformer_1 = require("@opentelemetry/otlp-transformer");
const browser_http_1 = require("@opentelemetry/otlp-exporter-base/browser-http");
const semconv_1 = require("../../semconv");
/**
 * Collector Metric Exporter for Web
 */
class OTLPMetricExporter extends OTLPMetricExporterBase_1.OTLPMetricExporterBase {
    _url;
    constructor(config) {
        super((0, browser_http_1.createLegacyOtlpBrowserExportDelegate)(config ?? {}, otlp_transformer_1.JsonMetricsSerializer, semconv_1.OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, otlp_transformer_1.MetricsExporterMetricsHelper, config?.selfObsMeterProvider, 'v1/metrics', { 'Content-Type': 'application/json' }), config);
        this._url = config?.url;
    }
    /**
     * Sets the meter provider to use to collect metrics for the exporter itself.
     * @experimental This method is experimental and is subject to breaking changes in minor releases.
     */
    setSelfObsMeterProvider(meterProvider) {
        this.setMetrics((0, browser_http_1.createLegacyOtlpBrowserExporterMetrics)(semconv_1.OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_METRIC_EXPORTER, otlp_transformer_1.MetricsExporterMetricsHelper, this._url, meterProvider));
    }
}
exports.OTLPMetricExporter = OTLPMetricExporter;
//# sourceMappingURL=OTLPMetricExporter.js.map