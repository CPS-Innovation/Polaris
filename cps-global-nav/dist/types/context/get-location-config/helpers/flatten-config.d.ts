import { AppLocationConfig, PathMatcher } from "../../LocationConfig";
export declare const flattenConfig: (appLocationConfigs: AppLocationConfig[]) => {
    pathRoot: string;
    path: string;
    pathMatcherValues: Omit<PathMatcher, "paths">;
}[];
