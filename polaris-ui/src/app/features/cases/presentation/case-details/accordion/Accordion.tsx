import { useReducer } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import classes from "./Accordion.module.scss";
import { AccordionHeader } from "./AccordionHeader";
import { AccordionSection } from "./AccordionSection";
import { buildInitialState, reducer } from "./reducer";
import { AccordionDocumentSection } from "./types";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";

type Props = {
  activeDocumentId: string;
  readUnreadData: string[];
  accordionState: AccordionDocumentSection[];
  handleOpenPdf: (caseDocument: {
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
};

export const Accordion: React.FC<Props> = ({
  activeDocumentId,
  accordionState: sections,
  readUnreadData,
  handleOpenPdf,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [state, dispatch] = useReducer(
    reducer,
    buildInitialState(sections.map((section) => section.sectionLabel))
  );

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
          handleToggleOpenSection={handleToggleOpenSection}
          handleOpenPdf={handleOpenPdf}
        />
      ))}
    </div>
  );
};
