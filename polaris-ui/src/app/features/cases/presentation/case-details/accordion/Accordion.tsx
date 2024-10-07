import { useReducer, useEffect, forwardRef, useImperativeHandle } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { NotesData } from "../../../domain/gateway/NotesData";
import classes from "./Accordion.module.scss";
import { AccordionHeader } from "./AccordionHeader";
import { AccordionSection } from "./AccordionSection";
import { buildInitialState, reducer, AccordionReducerState } from "./reducer";
import { AccordionDocumentSection } from "./types";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { Classification } from "../../../domain/gateway/PipelineDocument";
import { FeatureFlagData } from "../../../domain/FeatureFlagData";

type Props = {
  initialState: AccordionReducerState | null;
  activeDocumentId: string;
  readUnreadData: string[];
  accordionState: AccordionDocumentSection[];
  featureFlags: FeatureFlagData;
  handleOpenPdf: (caseDocument: {
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
  handleOpenPanel: (
    documentId: string,
    documentCategory: string,
    presentationFileName: string,
    type: "notes" | "rename",
    documentType: string,
    classification: Classification
  ) => void;
  accordionStateChangeCallback: (
    accordionCurrentState: AccordionReducerState
  ) => void;
  handleGetNotes: (documentId: string) => void;
  handleReclassifyDocument: (documentId: string) => void;
  notesData: NotesData[];
};
export type AccordionRef = {
  handleOpenAccordion: (documentId: string) => void;
};

export const Accordion = forwardRef<AccordionRef, Props>(
  (
    {
      initialState,
      activeDocumentId,
      accordionState: sections,
      readUnreadData,
      featureFlags,
      notesData,
      handleOpenPdf,
      handleOpenPanel,
      accordionStateChangeCallback,
      handleReclassifyDocument,
      handleGetNotes,
    },
    ref
  ) => {
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
    const handleOpenAccordion = (documentId: string) => {
      const section = sections.find((section) =>
        section.docs.find((doc) => doc.documentId === documentId)
      );
      if (section && !state.sections[`${section.sectionId}`]) {
        dispatch({
          type: "OPEN_CLOSE",
          payload: { id: section.sectionId, open: true },
        });
      }
    };

    useImperativeHandle(ref, () => ({
      handleOpenAccordion,
    }));

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
            featureFlags={featureFlags}
            handleToggleOpenSection={handleToggleOpenSection}
            handleOpenPdf={handleOpenPdf}
            handleOpenPanel={handleOpenPanel}
            handleGetNotes={handleGetNotes}
            handleReclassifyDocument={handleReclassifyDocument}
            notesData={notesData}
          />
        ))}
      </div>
    );
  }
);
