import { flattenConfig } from "./flatten-config";

export const isPathMatch = ({ pathRoot, path, pathMatcherValues: { matchedLinkCode, showSecondRow } }: ReturnType<typeof flattenConfig>[0], href: string, fullPath: string) => {
  const result = href.startsWith(pathRoot) && !!fullPath.match(path);
  console.debug(`Matching ${fullPath} against ${pathRoot} -> ${path}: result ${result} ${result ? `(matchedLinkCode: ${matchedLinkCode}, showSecondRow: ${showSecondRow} )` : ""}`);
  return result;
};
