import { useMemo, useCallback } from "react";
import { AccordionDocument } from "./AccordionDocument";
import classes from "./Accordion.module.scss";
import { ReactComponent as EmailIcon } from "../../../../../common/presentation/svgs/email.svg";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";

type Props = {
  sectionId: string;
  sectionLabel: string;
  docs: MappedCaseDocument[];
  isOpen: boolean;
  handleToggleOpenSection: (id: string, sectionLabel: string) => void;
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
  const groupIntoSubCategory = useCallback(() => {
    return docs.reduce((acc, doc) => {
      if (doc.presentationSubCategory) {
        if (!acc[`${doc.presentationSubCategory}`]) {
          acc[`${doc.presentationSubCategory}`] = [doc];
        } else {
          acc[`${doc.presentationSubCategory}`].push(doc);
        }
      }
      return acc;
    }, {} as Record<string, MappedCaseDocument[]>);
  }, [docs]);

  const subCategories = useMemo(() => {
    return groupIntoSubCategory();
  }, [groupIntoSubCategory]);

  const documentsWithLimitedView = () => {
    return docs.filter((doc) => doc.presentationFlags?.read !== "Ok");
  };

  const renderAccordionDocument = (
    docs: MappedCaseDocument[],
    subCategoryName?: string
  ) => {
    return (
      <>
        {subCategoryName && (
          <div className={classes.subCategory}>
            {subCategoryName === "Emails" && (
              <EmailIcon className={classes.emailIcon} />
            )}
            <h3>{subCategoryName}</h3>
          </div>
        )}
        <div className={`${classes["accordion-section-document"]}`}>
          {!docs.length ? (
            <div className={`${classes["accordion-section-no-document"]}`}>
              No Documents
            </div>
          ) : (
            <div>
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
      </>
    );
  };

  return (
    <div
      className={
        isOpen
          ? `${classes["accordion-section"]} ${classes.accordionSectionOpened}`
          : `${classes["accordion-section"]} ${classes.accordionSectionClosed}`
      }
      data-testid={`${formatTestIdText(sectionId)}`}
    >
      <button
        className={`${classes["accordion-section-header"]}`}
        onClick={() => handleToggleOpenSection(sectionId, sectionLabel)}
        aria-expanded={isOpen}
      >
        <h2 className="govuk-heading-s">{sectionLabel}</h2>
        <span className={`${classes["icon"]}`}></span>
      </button>
      <div className={`${classes["accordion-section-body"]}`}>
        {!!documentsWithLimitedView().length && (
          <span
            className={`${classes["accordion-section-read-warning"]}`}
            data-testid={`view-warning-${formatTestIdText(sectionId)}`}
          >
            Some documents for this case are only available in CMS
          </span>
        )}
        {Object.keys(subCategories).length
          ? Object.keys(subCategories)
              .sort()
              .map((subCategory, index) => (
                <div key={index}>
                  {renderAccordionDocument(
                    subCategories[subCategory],
                    subCategory
                  )}
                </div>
              ))
          : renderAccordionDocument(docs)}
      </div>
    </div>
  );
};
