import { MatchedPathMatcher, OnwardLinkDefinitions } from "../../LocationConfig";
export declare const replaceTagsInLinks: (linkDefinitions: OnwardLinkDefinitions, pathTags: MatchedPathMatcher["pathTags"]) => Partial<{
    tasks: string;
    cases: string;
    details: string;
    "case-materials": string;
    "bulk-um-classification": string;
    review: string;
    "cms-pre-charge-triage": string;
}>;
