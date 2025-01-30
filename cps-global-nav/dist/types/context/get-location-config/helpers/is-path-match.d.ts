import { flattenConfig } from "./flatten-config";
export declare const isPathMatch: ({ pathRoot, path, pathMatcherValues: { matchedLinkCode, showSecondRow } }: ReturnType<typeof flattenConfig>[0], href: string, fullPath: string) => boolean;
