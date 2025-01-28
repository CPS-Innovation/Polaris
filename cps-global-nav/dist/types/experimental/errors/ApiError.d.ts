export declare class ApiError extends Error {
    readonly name: string;
    readonly customMessage?: string;
    constructor(message: string, customMessage?: string);
}
