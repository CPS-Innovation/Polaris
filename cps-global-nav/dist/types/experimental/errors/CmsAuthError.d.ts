export declare class CmsAuthError extends Error {
    readonly name: string;
    readonly customMessage?: string;
    constructor(message: string, customMessage?: string);
}
