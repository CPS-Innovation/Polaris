import { useAppInsightsContext } from "@microsoft/applicationinsights-react-js";

type AppInsightsTrackEventNames =
  | "Search URN"
  | "Back to Search URN"
  | "Back to Find a Case"
  | "Open case"
  | "Search case documents from Case File"
  | "Search case documents from Document Search"
  | "Open All Folders"
  | "Close All Folders"
  | "Expand Doc Category"
  | "Collapse Doc Category"
  | "Open document from Case File"
  | "View 'x' more"
  | "Filter Doc Search Results"
  | "Open document from Document Search"
  | "View document tab"
  | "Open document in tab"
  | "Close document"
  | "Redact content"
  | "Remove Redact content"
  | "Remove All Redactions"
  | "Save All Redactions";

const eventDescription = {
  "Search URN":
    "User has clicked the 'Search' button on the 'Find a case' screen.",
  "Back to Search URN":
    "User has clicked the '<Back' link on the 'Find a case' search results screen",
  "Back to Find a Case":
    "User has clicked the '<Find a case' link on the top-left of the case file screen",
  "Open case":
    "User has clicked the hyperlinked URN on the 'Find a case' screen",
  "Search case documents from Case File":
    "User has clicked the 'Search' button in the left side menu on the 'Case File' screen.",
  "Search case documents from Document Search":
    "User has clicked the 'Search' button in the left side menu on the Document Search Results popup.",
  "Open All Folders":
    "User has clicked the 'Open all folders' link in the left side menu on the 'Find a case' screen",
  "Close All Folders":
    "User has clicked the 'Close all folders' link in the left side menu on the 'Find a case' screen",
  "Expand Doc Category":
    "User has clicked the toggle '+' against any of the document categories in the left side menu on the 'Find a case' screen",
  "Collapse Doc Category":
    "User has clicked the toggle '-' against any of the document categories in the left side menu on the 'Find a case' screen",
  "Open document from Case File":
    "User has clicked the hyperlinked document name in the left side menu on the 'Find a case' screen",
  "View 'x' more":
    "User has clicked the 'view <insert number here> more' link against any of the search result rows",
  "Filter Doc Search Results":
    "User has clicked the checkbox against any of the search filters",
  "Open document from Document Search":
    "User has clicked the hyperlinked document name in the left side menu",
  "View document tab":
    "User has clicked the tab in the tabs menu, which then brings that document tab into view",
  "Open document in tab":
    "User has clicked the '<doc name> (opens in new tab)' link",
  "Close document":
    "User has clicked the 'X' cross icon against the document tab",
  "Redact content":
    "User has clicked the 'Redact' green popup button within any document tab",
  "Remove Redact content":
    "User has clicked the 'Remove redaction' green popup button within any document tab",
  "Remove All Redactions":
    "User has clicked the 'Remove All Redactions' link in the document tab",
  "Save All Redactions":
    "User has clicked the 'Save All Redactions' green button in the document tab",
};
export const useAppInsightsTrackEvent = () => {
  const appInsights = useAppInsightsContext();

  const trackEvent = (
    name?: AppInsightsTrackEventNames,
    properties: { [key: string]: any } = {}
  ) => {
    if (!name) {
      return;
    }
    const description: string = eventDescription[name]
      ? eventDescription[name]
      : "";

    appInsights.trackEvent({
      name,
      properties: { ...properties, description },
    });
  };

  return trackEvent;
};
