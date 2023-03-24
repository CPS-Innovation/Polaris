import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../common/utils/dates";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";

import classes from "./Accordion.module.scss";

type Props = {
  caseDocument: MappedCaseDocument;
  handleOpenPdf: (caseDocument: {
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
};

export const AccordionDocument: React.FC<Props> = ({
  caseDocument,
  handleOpenPdf,
}) => {
  const canViewDocument = caseDocument.presentationFlags?.read === "Ok";
  return (
    <li className={`${classes["accordion-document-list-item"]}`}>
      <div className={`${classes["accordion-document-item-wrapper"]}`}>
        {canViewDocument ? (
          <LinkButton
            onClick={() => {
              handleOpenPdf(caseDocument);
            }}
            className={`${classes["accordion-document-link-button"]}`}
            dataTestId={`link-document-${caseDocument.documentId}`}
          >
            {caseDocument.presentationFileName}
          </LinkButton>
        ) : (
          <span
            className={`${classes["accordion-document-link-name"]}`}
            data-testid={`name-text-document-${caseDocument.documentId}`}
          >
            {caseDocument.presentationFileName}
          </span>
        )}
        <span className={`${classes["accordion-document-date"]}`}>
          {caseDocument.cmsFileCreatedDate &&
            formatDate(
              caseDocument.cmsFileCreatedDate,
              CommonDateTimeFormats.ShortDateTextMonth
            )}
        </span>
      </div>
      {!canViewDocument && (
        <span
          className={`${classes["accordion-document-read-warning"]}`}
          data-testid={`view-warning-document-${caseDocument.documentId}`}
        >
          Document only available on CMS
        </span>
      )}
    </li>
  );
};
