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
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
};
const formatTestIdText = (id: string) => {
  return id
    .replace(/\s+/g, " ")
    .split(" ")
    .map((word) => word.toLowerCase())
    .join("-");
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
    return docs.filter((doc) => doc.presentationFlags?.read !== "Ok");
  };
  return (
    <div
      className={`${classes["accordion-section"]}`}
      aria-expanded={isOpen}
      data-testid={`${formatTestIdText(sectionId)}`}
    >
      <button
        className={`${classes["accordion-section-header"]}`}
        onClick={() => handleToggleOpenSection(sectionId)}
      >
        <h2 className="govuk-heading-s">{sectionLabel}</h2>
        <span className={`${classes["icon"]}`}></span>
      </button>
      <div className={`${classes["accordion-section-body"]}`}>
        {!!documentsWithLimitedView().length && (
          <span data-testid={`view-warning-${formatTestIdText(sectionId)}`}>
            Some documents for this case are only available in CMS
          </span>
        )}

        <div className={`${classes["accordion-section-document"]}`}>
          {!docs.length ? (
            <div className={`${classes["accordion-section-no-document"]}`}>
              No Documents
            </div>
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
