export type LinkCode = "tasks" | "cases" | "details" | "case-materials" | "bulk-um-classification" | "review";

export type OnwardLinkDefinitions = Partial<{ [key in LinkCode]: string }>;

export type PathMatcher = { paths: string[]; matchedLinkCode: LinkCode; showSecondRow: boolean; onwardLinks: OnwardLinkDefinitions };

export type MatchedPathMatcher = Omit<PathMatcher, "paths"> & {
  pathTags?: { [key: string]: string };
};

export type AppLocationConfig = {
  pathRoots: string[];
  pathMatchers: PathMatcher[];
};

export const appLocationConfigs: AppLocationConfig[] = [
  {
    pathRoots: ["https://polaris-dev-notprod.cps.gov.uk/polaris-ui", "https://polaris-dev-cmsproxy.azurewebsites.net/polaris-ui", "http://localhost:3000/polaris-ui"],
    pathMatchers: [
      {
        paths: ["/polaris-ui/case-details/(?<urn>[^/+])/(?<caseId>[^d+])"],
        matchedLinkCode: "case-materials",
        showSecondRow: true,
        onwardLinks: { tasks: "https://cps-dev.outsystemsenterprise.com/WorkManagementApp/TaskList", cases: "https://cps-dev.outsystemsenterprise.com/WorkManagementApp/TaskList" },
      },
      {
        paths: ["/polaris-ui/", "/polaris-ui"],
        matchedLinkCode: "case-materials",
        showSecondRow: false,
        onwardLinks: { tasks: "https://cps-dev.outsystemsenterprise.com/WorkManagementApp/TaskList", cases: "https://cps-dev.outsystemsenterprise.com/WorkManagementApp/TaskList" },
      },
    ],
  },
  {
    pathRoots: ["http://localhost:3333"],
    pathMatchers: [
      {
        paths: ["/urns/(?<urn>[^/]+)/cases/(?<caseId>[^/]+)"],
        matchedLinkCode: "tasks",
        showSecondRow: true,
        onwardLinks: { tasks: "http://localhost:3333/?urn={urn}&caseId={caseId}", cases: "http://localhost:3333/?urn={urn}&caseId={caseId}" },
      },
      {
        paths: ["/", ""],
        matchedLinkCode: "tasks",
        showSecondRow: false,
        onwardLinks: { tasks: "http://localhost:3333/urns/12AB121212/cases/11112222", cases: "http://localhost:3333/urns/34AB343434/cases/333344444" },
      },
    ],
  },
];
