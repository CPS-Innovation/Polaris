export type LinkCode = "tasks" | "cases" | "details" | "case-materials" | "bulk-um-classification" | "review" | "cms-pre-charge-triage";

export type OnwardLinkDefinitions = Partial<{ [key in LinkCode]: string }>;

export type PathMatcher = { paths: string[]; matchedLinkCode: LinkCode; showSecondRow: boolean; onwardLinks: OnwardLinkDefinitions };

export type MatchedPathMatcher = Omit<PathMatcher, "paths"> & {
  href: string;
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
        paths: ["/polaris-ui/case-details/(?<urn>[^/]+)/(?<caseId>\\d+).*taskId=(?<taskId>\\d+)"],
        matchedLinkCode: "case-materials",
        showSecondRow: true,
        onwardLinks: {
          "tasks": "https://cps-dev.outsystemsenterprise.com/WorkManagementApp/TaskList",
          "cases": "https://cps-tst.outsystemsenterprise.com/WorkManagementApp/TaskList?IsCasesNavigation=true",
          "bulk-um-classification": "https://housekeeping-fn-staging.int.cps.gov.uk/api/init/{caseId}",
          "review": "https://cps-dev.outsystemsenterprise.com/CaseReview/RedirectCW?URN={urn}&CMSCaseId={caseId}",
          "cms-pre-charge-triage": "/api/navigate-cms?action=activate_task&screen=case_details&taskId={taskId}&caseId={caseId}&wId=MASTER",
          "details": "https://cps-tst.outsystemsenterprise.com/WorkManagementApp/CaseOverview?URN={urn}&CaseId={caseId}&IsDCFCase=false",
        },
      },
      {
        paths: ["/polaris-ui/case-details/(?<urn>[^/]+)/(?<caseId>\\d+)"],
        matchedLinkCode: "case-materials",
        showSecondRow: true,
        onwardLinks: {
          "tasks": "https://cps-dev.outsystemsenterprise.com/WorkManagementApp/TaskList",
          "cases": "https://cps-tst.outsystemsenterprise.com/WorkManagementApp/TaskList?IsCasesNavigation=true",
          "bulk-um-classification": "https://housekeeping-fn-staging.int.cps.gov.uk/api/init/{caseId}",
          "review": "https://cps-dev.outsystemsenterprise.com/CaseReview/RedirectCW?URN={urn}&CMSCaseId={caseId}",
          "details": "https://cps-tst.outsystemsenterprise.com/WorkManagementApp/CaseOverview?URN={urn}&CaseId={caseId}&IsDCFCase=false",
        },
      },
      {
        paths: ["/polaris-ui"],
        matchedLinkCode: "case-materials",
        showSecondRow: false,
        onwardLinks: {
          tasks: "https://cps-dev.outsystemsenterprise.com/WorkManagementApp/TaskList",
          cases: "https://cps-tst.outsystemsenterprise.com/WorkManagementApp/TaskList?IsCasesNavigation=true",
        },
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
        // add query params in alphabetical order e.g. ?a=1&b=2&c=1 rather than ?b=2&a=1&c=1
        onwardLinks: {
          "tasks": "http://localhost:3333/?urn={urn}&caseId={caseId}",
          "cases": "http://localhost:3333/?urn={urn}&caseId={caseId}",
          "bulk-um-classification": "/bulk-um-classification",
          "review": "/review",
          "details": "Please navigate to details page for case {caseId}",
        },
      },
      {
        paths: [""],
        matchedLinkCode: "tasks",
        showSecondRow: false,
        onwardLinks: {
          tasks: "http://localhost:3333/urns/12AB121212/cases/11112222",
          cases: "http://localhost:3333/urns/34AB343434/cases/333344444",
        },
      },
    ],
  },
];
