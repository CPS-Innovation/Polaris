import { forwardRef, useEffect, useImperativeHandle } from "react";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { NotesData } from "../../../domain/gateway/NotesData";
import classes from "./Accordion.module.scss";
import { AccordionHeader } from "./AccordionHeader";
import { AccordionSection } from "./AccordionSection";
import { AccordionData } from "./types";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { Classification } from "../../../domain/gateway/PipelineDocument";
import { FeatureFlagData } from "../../../domain/FeatureFlagData";
import { LocalDocumentState } from "../../../domain/LocalDocumentState";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

type Props = {
  handleToggleDocumentState: (
    urn: string | undefined,
    caseId: number | undefined,
    documentId: string,
    isUnused: boolean
  ) => void;
  documentsState: MappedCaseDocument[];
  activeDocumentId: string;
  readUnreadData: string[];
  accordionState: AccordionData;
  featureFlags: FeatureFlagData;
  handleOpenPdf: (caseDocument: {
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
  handleOpenPanel: (
    documentId: string,
    documentCategory: string,
    presentationTitle: string,
    type: "notes" | "rename",
    documentType: string,
    classification: Classification
  ) => void;
  handleGetNotes: (documentId: string) => void;
  handleReclassifyDocument: (documentId: string) => void;
  notesData: NotesData[];
  localDocumentState: LocalDocumentState;
  handleAccordionOpenClose: CaseDetailsState["handleAccordionOpenClose"];
  handleAccordionOpenCloseAll: CaseDetailsState["handleAccordionOpenCloseAll"];
  hkDocumentId: string | undefined;
  handleUpdateDCFAction: (mode: any) => void;
  mode?: boolean | string;
};
export type AccordionRef = {
  handleOpenAccordion: (documentId: string) => void;
};

export const Accordion = forwardRef<AccordionRef, Props>(
  (
    {
      activeDocumentId,
      documentsState,
      accordionState: { sections, isAllOpen, sectionsOpenStatus },
      readUnreadData,
      featureFlags,
      notesData,
      localDocumentState,
      handleOpenPdf,
      handleOpenPanel,
      handleReclassifyDocument,
      handleGetNotes,
      handleToggleDocumentState,
      handleAccordionOpenClose,
      handleAccordionOpenCloseAll,
      hkDocumentId,
      mode,
    },
    ref
  ) => {
    const trackEvent = useAppInsightsTrackEvent();

    // handleUpdateDCFAction();

    const handleToggleOpenAll = () => {
      if (isAllOpen) {
        trackEvent("Close All Folders");
      } else {
        trackEvent("Open All Folders");
      }
      handleAccordionOpenCloseAll(!isAllOpen);
    };
    const handleToggleOpenSection = (id: string, sectionLabel: string) => {
      if (sectionsOpenStatus[id]) {
        trackEvent("Collapse Doc Category", { categoryName: sectionLabel });
      } else {
        trackEvent("Expand Doc Category", { categoryName: sectionLabel });
      }
      handleAccordionOpenClose(id, !sectionsOpenStatus[id]);
    };
    const handleOpenAccordion = (documentId: string) => {
      const section = sections.find((section) =>
        section.docs.find((doc) => doc.documentId === documentId)
      );
      if (section && !sectionsOpenStatus[`${section.sectionId}`]) {
        handleAccordionOpenClose(section.sectionId, true);
      }
    };

    useImperativeHandle(ref, () => ({
      handleOpenAccordion,
      // mode,
    }));

    useEffect(() => {
      handleOpenAccordion(hkDocumentId as string);
    }, [hkDocumentId]);

    useEffect(() => {
      const st = setTimeout(() => {
        const panel = document.querySelector(
          '[data-document-active="true"]'
        ) as HTMLElement;
        if (panel) {
          panel.scrollIntoView({
            behavior: "smooth",
            block: "end",
            inline: "end",
          });
        }
      }, 0);

      return () => clearTimeout(st);
    }, []);

    useEffect(() => {
      hkDocumentId && handleOpenPdf({ documentId: hkDocumentId });
    }, [hkDocumentId]);

    return (
      <div className={`${classes.accordion}`}>
        <AccordionHeader
          isAllOpen={isAllOpen}
          handleToggleOpenAll={handleToggleOpenAll}
        />
        {sections.map(({ sectionId, sectionLabel, docs }) => (
          <AccordionSection
            key={sectionId}
            sectionId={sectionId}
            sectionLabel={sectionLabel}
            documentsState={documentsState}
            docs={docs}
            isOpen={sectionsOpenStatus[sectionId]}
            readUnreadData={readUnreadData}
            activeDocumentId={activeDocumentId}
            featureFlags={featureFlags}
            handleToggleOpenSection={handleToggleOpenSection}
            handleOpenPdf={handleOpenPdf}
            handleOpenPanel={handleOpenPanel}
            handleGetNotes={handleGetNotes}
            handleReclassifyDocument={handleReclassifyDocument}
            notesData={notesData}
            localDocumentState={localDocumentState}
            handleToggleDocumentState={handleToggleDocumentState}
            hkDocumentId={hkDocumentId}
          />
        ))}
      </div>
    );
  }
);
