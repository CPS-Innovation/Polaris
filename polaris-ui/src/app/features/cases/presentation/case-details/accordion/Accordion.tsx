import { useReducer } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import classes from "./Accordion.module.scss";
import { AccordionHeader } from "./AccordionHeader";
import { AccordionSection } from "./AccordionSection";
import { buildInitialState, reducer } from "./reducer";
import { AccordionDocumentSection } from "./types";

type Props = {
  accordionState: AccordionDocumentSection[];
  handleOpenPdf: (caseDocument: {
    tabSafeId: string;
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
};

export const Accordion: React.FC<Props> = ({
  accordionState: sections,
  handleOpenPdf,
}) => {
  console.log("sections>>>>>", sections);
  const [state, dispatch] = useReducer(
    reducer,
    buildInitialState(sections.map((section) => section.sectionLabel))
  );

  const handleToggleOpenAll = () =>
    dispatch({ type: "OPEN_CLOSE_ALL", payload: !state.isAllOpen });
  const handleToggleOpenSection = (id: string) =>
    dispatch({
      type: "OPEN_CLOSE",
      payload: { id, open: !state.sections[id] },
    });

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
          handleToggleOpenSection={handleToggleOpenSection}
          handleOpenPdf={handleOpenPdf}
        />
      ))}
    </div>
  );
};
