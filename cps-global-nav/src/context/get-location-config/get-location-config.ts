import { appLocationConfigs, MatchedPathMatcher } from "../LocationConfig";
import { buildSortedFullPath } from "./helpers/build-sorted-full-path";
import { flattenConfig } from "./helpers/flatten-config";
import { isPathMatch } from "./helpers/is-path-match";
import { replaceTagsInLinks } from "./helpers/replace-tags-in-links";

export const getLocationConfig = ({ location: { href } }: Window): MatchedPathMatcher => {
  const fullPath = buildSortedFullPath(location);
  const flatConfigs = flattenConfig(appLocationConfigs);
  const config = flatConfigs.find(config => isPathMatch(config, href, fullPath));

  if (!config) {
    return null;
  }

  const {
    path,
    pathMatcherValues: { onwardLinks, ...rest },
  } = config;

  const pathTags = fullPath.match(path).groups;

  return {
    pathTags,
    onwardLinks: replaceTagsInLinks(onwardLinks, pathTags),
    href,
    ...rest,
  };
};
