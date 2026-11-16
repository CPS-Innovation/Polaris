export interface IExporterMetricsHelper<Internal> {
    name: 'span' | 'metric_data_point' | 'log';
    countItems: (request: Internal) => number;
}
//# sourceMappingURL=i-exporter-metrics-helper.d.ts.map