import { AccordionDocument } from "./AccordionDocument";
import classes from "./Accordion.module.scss";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";

type Props = {
  sectionId: string;
  sectionLabel: string;
  docs: MappedCaseDocument[];
  isOpen: boolean;
  handleToggleOpenSection: (id: string) => void;
  handleOpenPdf: (caseDocument: {
    tabSafeId: string;
    documentId: CaseDocumentViewModel["documentId"];
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
  const documentsWithLimitedView = () => {
    return docs.filter((doc) => doc.presentationStatuses?.viewStatus !== "Ok");
  };
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
        {!!documentsWithLimitedView().length && (
          <span> Some documents for this case are only available in CMS</span>
        )}

        <div className={`${classes["accordion-section-no-document"]}`}>
          {!docs.length ? (
            <div> No Documents</div>
          ) : (
            <div>
              <span className={`${classes["accordion-document-date-title"]}`}>
                Date added
              </span>

              <ul className={`${classes["accordion-document-list"]}`}>
                {docs.map((caseDocument) => (
                  <AccordionDocument
                    key={caseDocument.documentId}
                    caseDocument={caseDocument}
                    handleOpenPdf={handleOpenPdf}
                  />
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
