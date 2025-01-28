export type AccordionDocumentSection = {
  sectionId: string;
  sectionLabel: string;
  docs: { documentId: string }[];
};

export type AccordionData = {
  sectionsOpenStatus: { [key: string]: boolean };
  isAllOpen: boolean;
  sections: AccordionDocumentSection[];
};
