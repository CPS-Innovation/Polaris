"use strict";
/*
 * Copyright The OpenTelemetry Authors
 * SPDX-License-Identifier: Apache-2.0
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.OTLPTraceExporter = void 0;
const otlp_exporter_base_1 = require("@opentelemetry/otlp-exporter-base");
const otlp_transformer_1 = require("@opentelemetry/otlp-transformer");
const node_http_1 = require("@opentelemetry/otlp-exporter-base/node-http");
const semconv_1 = require("../../semconv");
/**
 * Collector Trace Exporter for Node with protobuf
 */
class OTLPTraceExporter extends otlp_exporter_base_1.OTLPExporterBase {
    constructor(config = {}) {
        super((0, node_http_1.createOtlpHttpExportDelegate)((0, node_http_1.convertLegacyHttpOptions)(config, 'TRACES', 'v1/traces', {
            'Content-Type': 'application/x-protobuf',
        }), otlp_transformer_1.ProtobufTraceSerializer, semconv_1.OTEL_COMPONENT_TYPE_VALUE_OTLP_HTTP_SPAN_EXPORTER, otlp_transformer_1.TraceExporterMetricsHelper, config.selfObsMeterProvider));
    }
}
exports.OTLPTraceExporter = OTLPTraceExporter;
//# sourceMappingURL=OTLPTraceExporter.js.map