import { useAppInsightsContext } from "@microsoft/applicationinsights-react-js";
import { useEffect } from "react";
import { useParams } from "react-router-dom";

type AppInsightsTrackEventNames =
  | "Search URN"
  | "Back To Search URN"
  | "Back To Find A Case"
  | "Open Case"
  | "Search Case Documents From Case File"
  | "Search Case Documents From Document Search"
  | "Open All Folders"
  | "Close All Folders"
  | "Expand Doc Category"
  | "Collapse Doc Category"
  | "Open Document From Case File"
  | "View 'x' More"
  | "Filter Doc Search Results"
  | "Open Document From Document Search"
  | "View Document Tab"
  | "Open Document In Tab"
  | "Close Document"
  | "Redact Content"
  | "Remove Redact Content"
  | "Remove All Redactions"
  | "Save All Redactions"
  | "Report Document Issue"
  | "Uncategorised Document"
  | "Categorised Documents Count";

const eventDescription: { [key in AppInsightsTrackEventNames]: string } = {
  "Search URN":
    "User has clicked the 'Search' button on the 'Find a case' screen.",
  "Back To Search URN":
    "User has clicked the '<Back' link on the 'Find a case' search results screen",
  "Back To Find A Case":
    "User has clicked the '<Find a case' link on the top-left of the case file screen",
  "Open Case":
    "User has clicked the hyperlinked URN on the 'Find a case' screen",
  "Search Case Documents From Case File":
    "User has clicked the 'Search' button in the left side menu on the 'Case File' screen.",
  "Search Case Documents From Document Search":
    "User has clicked the 'Search' button in the left side menu on the Document Search Results popup.",
  "Open All Folders":
    "User has clicked the 'Open all folders' link in the left side menu on the 'Find a case' screen",
  "Close All Folders":
    "User has clicked the 'Close all folders' link in the left side menu on the 'Find a case' screen",
  "Expand Doc Category":
    "User has clicked the toggle '+' against any of the document categories in the left side menu on the 'Find a case' screen",
  "Collapse Doc Category":
    "User has clicked the toggle '-' against any of the document categories in the left side menu on the 'Find a case' screen",
  "Open Document From Case File":
    "User has clicked the hyperlinked document name in the left side menu on the 'Find a case' screen",
  "View 'x' More":
    "User has clicked the 'view <insert number here> more' link against any of the search result rows",
  "Filter Doc Search Results":
    "User has clicked the checkbox against any of the search filters",
  "Open Document From Document Search":
    "User has clicked the hyperlinked document name in the search results",
  "View Document Tab":
    "User has clicked the tab in the tabs menu, which then brings that document tab into view",
  "Open Document In Tab":
    "User has clicked the '<doc name> (opens in new tab)' link",
  "Close Document":
    "User has clicked the 'X' cross icon against the document tab",
  "Redact Content":
    "User has clicked the 'Redact' green popup button within any document tab",
  "Remove Redact Content":
    "User has clicked the 'Remove redaction' green popup button within any document tab",
  "Remove All Redactions":
    "User has clicked the 'Remove All Redactions' link in the document tab",
  "Save All Redactions":
    "User has clicked the 'Save All Redactions' green button in the document tab",
  "Report Document Issue": "User reporting issues with a document",
  "Uncategorised Document": "Uncategorised document",
  "Categorised Documents Count": "Documents count under particular category",
};
const useAppInsightsTrackEvent = () => {
  const { id: caseId, urn } = useParams<{ id: string; urn: string }>();
  const appInsights = useAppInsightsContext();

  const trackEvent = (
    name: AppInsightsTrackEventNames,
    properties: { [key: string]: any } = {}
  ) => {
    if (!name || !appInsights?.trackEvent) {
      return;
    }
    const description: string = eventDescription[name]
      ? eventDescription[name]
      : "";
    const generalProperties = urn
      ? {
          urn: urn,
          caseId: caseId,
        }
      : {};
    appInsights.trackEvent({
      name,
      properties: { ...properties, description, ...generalProperties },
    });
  };

  return trackEvent;
};

const useAppInsightsTrackPageView = (name: string) => {
  const appInsights = useAppInsightsContext();

  const trackPageView = (name: string) => {
    if (!name || !appInsights?.trackPageView) {
      return;
    }
    appInsights.trackPageView({ name });
  };

  useEffect(() => {
    trackPageView(name);
  }, []);
};

export { useAppInsightsTrackEvent, useAppInsightsTrackPageView };
