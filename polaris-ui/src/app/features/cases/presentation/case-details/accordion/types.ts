import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";

export type AccordionDocumentSection = {
  sectionId: string;
  sectionLabel: string;
  docs: MappedCaseDocument[];
};
