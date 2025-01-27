import { AppLocationConfig, appLocationConfigs, MatchedPathMatcher, OnwardLinkDefinitions, PathMatcher } from "./LocationConfig";

const flattenConfig = (appLocationConfigs: AppLocationConfig[]) => {
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

const isPathMatch = ({ pathRoot, path }: ReturnType<typeof flattenConfig>[0], { href, pathname, search }: Location) => {
  const result = href.startsWith(pathRoot) && !!(pathname + search).match(path);
  console.debug(`Matching ${pathname + search} against ${pathRoot} -> ${path}: result ${result}`);
  return result;
};

const replaceTags = (source: string, pathTags: MatchedPathMatcher["pathTags"]) =>
  Object.keys(pathTags).reduce((acc, curr) => {
    const tag = `{${curr}}`;
    console.debug(`Replacing ${tag} in ${acc} with ${pathTags[curr]}`);
    return acc.replace(new RegExp(tag, "g"), pathTags[curr]);
  }, source);

const replaceTagsInOnwardLinks = (linkDefinitions: OnwardLinkDefinitions, pathTags: MatchedPathMatcher["pathTags"]) => {
  if (!pathTags || !Object.keys(pathTags).length) {
    return linkDefinitions;
  }
  return Object.keys(linkDefinitions).reduce((acc, curr) => {
    acc[curr] = replaceTags(linkDefinitions[curr], pathTags);
    return acc;
  }, {} as OnwardLinkDefinitions);
};

export const getLocationConfig = ({ location: { pathname, search } }: Window): MatchedPathMatcher => {
  const flatConfigs = flattenConfig(appLocationConfigs);
  const config = flatConfigs.find(config => isPathMatch(config, location));
  if (!config) {
    return null;
  }

  const {
    path,
    pathMatcherValues: { onwardLinks, ...rest },
  } = config;
  const pathTags = (pathname + search).match(path).groups;

  return {
    pathTags,
    onwardLinks: replaceTagsInOnwardLinks(onwardLinks, pathTags),
    ...rest,
  };
};
