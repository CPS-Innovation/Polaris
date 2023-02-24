import { AccordionDocument } from "./AccordionDocument";
import classes from "./Accordion.module.scss";
import { AccordionNoDocuments } from "./AccordionNoDocuments";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";

type Props = {
  sectionId: string;
  sectionLabel: string;
  docs: MappedCaseDocument[];
  isOpen: boolean;
  handleToggleOpenSection: (id: string) => void;
  handleOpenPdf: (caseDocument: {
    tabSafeId: string;
    documentId: number;
  }) => void;
};

export const AccordionSection: React.FC<Props> = ({
  sectionId,
  sectionLabel,
  docs,
  isOpen,
  handleToggleOpenSection,
  handleOpenPdf,
}) => {
  return (
    <div className={`${classes["accordion-section"]}`} aria-expanded={isOpen}>
      <div
        className={`${classes["accordion-section-header"]}`}
        role="button"
        tabIndex={0}
        onClick={() => handleToggleOpenSection(sectionId)}
      >
        <h2 className="govuk-heading-s">{sectionLabel}</h2>
        <span className={`${classes["icon"]}`}></span>
      </div>
      <div className={`${classes["accordion-section-body"]}`}>
        <table className="govuk-table">
          {!docs.length ? (
            <tbody>
              <AccordionNoDocuments />
            </tbody>
          ) : (
            <>
              <thead>
                <tr className="govuk-table__row">
                  <th scope="col" className="govuk-table__header"></th>
                  <th
                    scope="col"
                    className="govuk-table__header govuk-body-s"
                    style={{ fontWeight: 400 }}
                  >
                    Date added
                  </th>
                </tr>
              </thead>
              <tbody>
                {docs.map((caseDocument) => (
                  <AccordionDocument
                    key={caseDocument.documentId}
                    caseDocument={caseDocument}
                    handleOpenPdf={handleOpenPdf}
                  />
                ))}
              </tbody>
            </>
          )}
        </table>
      </div>
    </div>
  );
};
