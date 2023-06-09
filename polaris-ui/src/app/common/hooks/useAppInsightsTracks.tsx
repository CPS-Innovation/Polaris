import { useAppInsightsContext } from "@microsoft/applicationinsights-react-js";
import { useEffect } from "react";
import { useParams } from "react-router-dom";

type AppInsightsTrackEventNames =
  | "Search URN"
  | "Back To Search URN"
  | "Back to Case Search Results"
  | "Open Case"
  | "Search Case Documents From Case Details"
  | "Search Case Documents From Document Search"
  | "Open All Folders"
  | "Close All Folders"
  | "Expand Doc Category"
  | "Collapse Doc Category"
  | "Open Document From Case Details"
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
    "User has clicked the 'Search' button on the 'Case Search' screen.",
  "Back To Search URN":
    "User has clicked the '<Back' link on the 'Case Search Results' screen.",
  "Back to Case Search Results":
    "User has clicked the '<Find a case' link on the top-left of the 'Case Details' screen.",
  "Open Case":
    "User has clicked the hyperlinked URN on the 'Case Search Results' screen.",
  "Search Case Documents From Case Details":
    "User has clicked the 'Search' button in the left side menu on the 'Case Details' screen.",
  "Search Case Documents From Document Search":
    "User has clicked the 'Search' button on the Document Search Results popup.",
  "Open All Folders":
    "User has clicked the 'Open all folders' link in the left side menu on the 'Case Details' screen.",
  "Close All Folders":
    "User has clicked the 'Close all folders' link in the left side menu on the 'Case Details' screen.",
  "Expand Doc Category":
    "User has clicked the toggle '+' against any of the document categories in the left side menu on the 'Case Details' screen.",
  "Collapse Doc Category":
    "User has clicked the toggle '-' against any of the document categories in the left side menu on the 'Case Details' screen",
  "Open Document From Case Details":
    "User has clicked the hyperlinked document name in the left side menu on the 'Case Details' screen.",
  "View 'x' More":
    "User has clicked the 'view 'x' more' link against any of the search result rows on the Document Search Results popup.",
  "Filter Doc Search Results":
    "User has clicked the checkbox against any of the search filters on the Document Search Results popup.",
  "Open Document From Document Search":
    "User has clicked the hyperlinked document name on the Document Search Results popup.",
  "View Document Tab":
    "User has clicked a tab in the tabs menu, bringing that document tab into view.",
  "Open Document In Tab":
    "User has clicked the '<doc name> (opens in new tab)' link, to open document in a new browser tab.",
  "Close Document":
    "User has clicked the 'X' cross icon to close the document tab.",
  "Redact Content":
    "User has clicked the 'Redact' green popup button in a document tab, to mark a single selection for redaction.",
  "Remove Redact Content":
    "User has clicked the 'Remove redaction' green popup button in a document tab, to mark a single selection for removal of a redaction.",
  "Remove All Redactions":
    "User has clicked the 'Remove All Redactions' link in a document tab.",
  "Save All Redactions":
    "User has clicked the 'Save All Redactions' green button in a document tab.",
  "Report Document Issue":
    "User has clicked to report an issue with a document.",
  "Uncategorised Document":
    "Action auto-triggered. Action not initiated by a user. Log an Uncategorised document after loading the Case Details screen.",
  "Categorised Documents Count":
    "Action auto-triggered. Action not initiated by a user. Log the document count by category after loading the Case Details screen.",
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
