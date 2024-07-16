import { useReducer, useEffect } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { NotesData } from "../../../domain/gateway/NotesData";
import classes from "./Accordion.module.scss";
import { AccordionHeader } from "./AccordionHeader";
import { AccordionSection } from "./AccordionSection";
import { buildInitialState, reducer, AccordionReducerState } from "./reducer";
import { AccordionDocumentSection } from "./types";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";

type Props = {
  initialState: AccordionReducerState | null;
  lastFocusDocumentId: string;
  activeDocumentId: string;
  readUnreadData: string[];
  accordionState: AccordionDocumentSection[];
  showNotesFeature: boolean;
  handleOpenPdf: (caseDocument: {
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
  handleOpenPanel: (
    documentId: string,
    documentCategory: string,
    presentationFileName: string,
    type: "notes" | "rename"
  ) => void;
  accordionStateChangeCallback: (
    accordionCurrentState: AccordionReducerState
  ) => void;
  handleGetNotes: (documentId: string) => void;
  notesData: NotesData[];
};

export const Accordion: React.FC<Props> = ({
  initialState,
  lastFocusDocumentId,
  activeDocumentId,
  accordionState: sections,
  readUnreadData,
  showNotesFeature,
  notesData,
  handleOpenPdf,
  handleOpenPanel,
  accordionStateChangeCallback,
  handleGetNotes,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [state, dispatch] = useReducer(
    reducer,
    initialState !== null
      ? initialState
      : buildInitialState(sections.map((section) => section.sectionLabel))
  );

  useEffect(() => {
    accordionStateChangeCallback(state);
  }, [state, accordionStateChangeCallback]);

  const handleToggleOpenAll = () => {
    if (state.isAllOpen) {
      trackEvent("Close All Folders");
    } else {
      trackEvent("Open All Folders");
    }
    dispatch({ type: "OPEN_CLOSE_ALL", payload: !state.isAllOpen });
  };
  const handleToggleOpenSection = (id: string, sectionLabel: string) => {
    if (state.sections[id]) {
      trackEvent("Collapse Doc Category", { categoryName: sectionLabel });
    } else {
      trackEvent("Expand Doc Category", { categoryName: sectionLabel });
    }
    dispatch({
      type: "OPEN_CLOSE",
      payload: { id, open: !state.sections[id] },
    });
  };

  return (
    <div className={`${classes.accordion}`}>
      <AccordionHeader
        isAllOpen={state.isAllOpen}
        handleToggleOpenAll={handleToggleOpenAll}
      />
      {sections.map(({ sectionId, sectionLabel, docs }) => (
        <AccordionSection
          key={sectionId}
          sectionId={sectionId}
          sectionLabel={sectionLabel}
          docs={docs}
          isOpen={state.sections[sectionId]}
          readUnreadData={readUnreadData}
          activeDocumentId={activeDocumentId}
          showNotesFeature={showNotesFeature}
          handleToggleOpenSection={handleToggleOpenSection}
          handleOpenPdf={handleOpenPdf}
          handleOpenPanel={handleOpenPanel}
          lastFocusDocumentId={lastFocusDocumentId}
          handleGetNotes={handleGetNotes}
          notesData={notesData}
        />
      ))}
    </div>
  );
};
