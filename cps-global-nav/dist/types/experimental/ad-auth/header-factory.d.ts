export declare const correlationId: (existingCorrelationId?: string) => {
    "Correlation-Id": string;
};
export declare const auth: () => Promise<{
    Authorization: string;
}>;
export declare const buildHeaders: (knownCorrelationId?: string) => Promise<Record<string, string>>;
