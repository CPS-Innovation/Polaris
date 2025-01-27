export type LinkCode = "tasks" | "cases" | "details" | "case-materials" | "bulk-um-classification" | "review";
export type OnwardLinkDefinitions = Partial<{
    [key in LinkCode]: string;
}>;
export type PathMatcher = {
    paths: string[];
    matchedLinkCode: LinkCode;
    showSecondRow: boolean;
    onwardLinks: OnwardLinkDefinitions;
};
export type MatchedPathMatcher = Omit<PathMatcher, "paths"> & {
    pathTags?: {
        [key: string]: string;
    };
};
export type AppLocationConfig = {
    pathRoots: string[];
    pathMatchers: PathMatcher[];
};
export declare const appLocationConfigs: AppLocationConfig[];
