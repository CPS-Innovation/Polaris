import { AppLocationConfig, PathMatcher } from "../../LocationConfig";

export const flattenConfig = (appLocationConfigs: AppLocationConfig[]) => {
  const result = [] as {
    pathRoot: string;
    path: string;
    pathMatcherValues: Omit<PathMatcher, "paths">;
  }[];

  for (const appLocationConfig of appLocationConfigs) {
    for (const pathRoot of appLocationConfig.pathRoots) {
      for (const { paths, ...pathMatcherValues } of appLocationConfig.pathMatchers) {
        for (const path of paths) {
          result.push({
            pathRoot,
            path,
            pathMatcherValues,
          });
        }
      }
    }
  }
  return result;
};
