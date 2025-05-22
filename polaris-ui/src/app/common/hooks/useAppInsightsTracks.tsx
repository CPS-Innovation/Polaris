import { useAppInsightsContext } from "@microsoft/applicationinsights-react-js";
import { useCallback, useEffect } from "react";
import { useParams } from "react-router-dom";
import { useUserDetails } from "../../../app/auth";
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
  | "Uncategorised Document"
  | "Categorised Documents Count"
  | "Open Documents Count"
  | "Failed Default Mapping Redaction Log"
  | "Save Redaction Log"
  | "Save Redaction Log Under Over"
  | "Open Under Over Redaction Log"
  | "Close Under Over Redaction Log"
  | "View Full Screen"
  | "Exit Full Screen"
  | "Redact Area Tool On"
  | "Redact Area Tool Off"
  | "Save Redaction Log Error"
  | "Add Unsaved Redactions"
  | "Ignore Unsaved Redactions"
  | "Document Checkout Error"
  | "Document Checked Out By Another User Error"
  | "Open Notes"
  | "Add Note"
  | "Add Note Error"
  | "Notes Document Mismatch Ok"
  | "Notes Document Mismatch Cancel"
  | "Ignore Redaction Suggestion"
  | "Cancel Save Redaction Suggestion Warning"
  | "Save Rename Document"
  | "Save Rename Document Error"
  | "Save Reclassify"
  | "Save Reclassify Error"
  | "Delete Page"
  | "Undo Delete Page"
  | "Save Redaction Error"
  | "Save Rotation Error"
  | "Notification Panel Opened"
  | "Document Opened from Notification"
  | "Notification Cleared"
  | "All Notifications Cleared"
  | "Notifications Arrived"
  | "Save Rotation"
  | "Remove All Rotations"
  | "Rotate Page"
  | "Undo Rotate Page"
  | "Rotate Page Right"
  | "Rotate Page Left"
  | "Update Document Evidential Status"
  | "Ignore Saved Or Unsaved Redactions Modal Window"
  | "Copy Text Content"
  | "Search Results Available Link";

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
  "Uncategorised Document":
    "Action auto-triggered. Action not initiated by a user. Log an Uncategorised document after loading the Case Details screen.",
  "Categorised Documents Count":
    "Action auto-triggered. Action not initiated by a user. Log the document count by category after loading the Case Details screen.",
  "Open Documents Count": "Number of documents opened at a time",
  "Failed Default Mapping Redaction Log":
    "Reporting if default mapping failed to find correct default value for at least one of the fields",
  "Save Redaction Log":
    "User has clicked save and close button in the under redaction log form",
  "Save Redaction Log Under Over":
    "User has clicked save and close button in the under over redaction log form",
  "Open Under Over Redaction Log":
    "User has clicked on the Log an Under/Over redaction button in the action drop down menu",
  "Close Under Over Redaction Log":
    "User has clicked on the close button of the under over redaction log modal button",
  "View Full Screen": "User has clicked on the view full screen button",
  "Exit Full Screen": "User has clicked on the exit full screen button",
  "Redact Area Tool On":
    "User has clicked on the Redact Area Tool button, to turn on area only redaction",
  "Redact Area Tool Off":
    "User has clicked on the Redact Area Tool button, to turn off area only redaction",
  "Save Redaction Log Error":
    "The attempt to save the data log failed, prompting the display of an error message to the user",
  "Add Unsaved Redactions":
    "User has clicked apply button on the unsaved redaction modal",
  "Ignore Unsaved Redactions":
    "User has clicked ignore button on the unsaved redaction modal",
  "Document Checkout Error":
    "The attempt to checkout a document failed, prompting the display of an error message to the user",
  "Document Checked Out By Another User Error":
    "The attempt to checkout a document failed, as it is already checked by another user, prompting the display of an error message to the user",
  "Open Notes": "User has clicked open notes button in the accordion document",
  "Add Note": "User has clicked Add note button in the notes panel",
  "Add Note Error":
    "The attempt to add a new note for a document failed, prompting the display of an error message to the user",
  "Notes Document Mismatch Ok":
    "User has clicked Ok button in the document mismatch modal",
  "Notes Document Mismatch Cancel":
    "User has clicked cancel or close button in the document mismatch modal",
  "Ignore Redaction Suggestion":
    "User has clicked 'Ignore' or 'Ignore all' button, to ignore any search pii redaction suggestions",
  "Cancel Save Redaction Suggestion Warning":
    "User has clicked 'cancel' or close button on the save redaction suggestions warning modal",
  "Save Rename Document":
    "User document rename request sent to the server after successful UI validation",
  "Save Rename Document Error":
    "Attempt to rename a document failed and displayed error message to the user",
  "Save Reclassify":
    "User document reclassify request sent to the server after successful UI validation",
  "Save Reclassify Error":
    "Attempt to reclassify a document failed and displayed error message to the user",
  "Delete Page": "User has marked a page for deletion",
  "Undo Delete Page": "User selects to undo the page deletion",
  "Save Redaction Error":
    "Attempt to save redaction failed and displayed error message to the user",
  "Notifications Arrived": "Notifications Arrived",
  "Notification Panel Opened": "Notification Panel Opened",
  "Document Opened from Notification": "Document Opened from Notification",
  "Notification Cleared": "Notification Cleared",
  "All Notifications Cleared": "All Notifications Cleared",
  "Save Rotation": "User has clicked on the 'Save all rotations' button",
  "Save Rotation Error":
    "Attempt to save rotations failed and displayed error message to the user",
  "Remove All Rotations": "User has clicked 'Remove All Rotations' button",
  "Rotate Page": "User has clicked page 'Rotate page' button",
  "Undo Rotate Page": "User has cancelled page rotate",
  "Rotate Page Right": "User clicked 'Rotate page right' button",
  "Rotate Page Left": "User clicked 'Rotate page left' button",
  "Update Document Evidential Status":
    "User has changed the evidential status between used and unused",
  "Ignore Saved Or Unsaved Redactions Modal Window":
    "User has closed used or unused modal window",
  "Copy Text Content": "User clicked 'Copy' button",
  "Search Results Available Link":
    "User clicked the search results available link",
};
const useAppInsightsTrackEvent = () => {
  const { id: caseId, urn } = useParams<{ id: string; urn: string }>();
  const appInsights = useAppInsightsContext();
  const userDetails = useUserDetails();

  const trackEvent = useCallback(
    (
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
        properties: {
          ...properties,
          description,
          ...generalProperties,
          ...userDetails,
        },
      });
    },
    [appInsights, caseId, urn, userDetails]
  );

  return trackEvent;
};

const useAppInsightsTrackPageView = (name: string) => {
  const appInsights = useAppInsightsContext();
  const userDetails = useUserDetails();

  const trackPageView = (name: string) => {
    if (!name || !appInsights?.trackPageView) {
      return;
    }
    appInsights.trackPageView({ name, properties: { ...userDetails } });
  };

  useEffect(() => {
    trackPageView(name);
  }, []);
};

export { useAppInsightsTrackEvent, useAppInsightsTrackPageView };
