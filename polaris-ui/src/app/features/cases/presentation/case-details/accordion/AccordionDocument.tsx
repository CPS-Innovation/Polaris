import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../common/utils/dates";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";

import classes from "./Accordion.module.scss";

type Props = {
  caseDocument: MappedCaseDocument;
  handleOpenPdf: (caseDocument: {
    tabSafeId: string;
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
};

export const AccordionDocument: React.FC<Props> = ({
  caseDocument,
  handleOpenPdf,
}) => {
  const canViewDocument =
    caseDocument.presentationStatuses?.viewStatus === "Ok";
  return (
    <li className={`${classes["accordion-document-list-item"]}`}>
      <div className={`${classes["accordion-document-item-wrapper"]}`}>
        {canViewDocument ? (
          <a
            className={`${classes["accordion-document-link-button"]}`}
            href={`#${caseDocument.tabSafeId}`}
            onClick={(ev) => {
              handleOpenPdf(caseDocument);
            }}
            data-testid={`link-document-${caseDocument.documentId}`}
          >
            {caseDocument.presentationFileName}
          </a>
        ) : (
          <span className={`${classes["accordion-document-link-name"]}`}>
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
      {!canViewDocument && <span>Document only available on CMS</span>}
    </li>
  );

  // <tr className="govuk-table__row">
  //   <td
  //     className="govuk-table__cell govuk-body-s openMe"
  //     style={{ wordWrap: "break-word" }}
  //   >
  //     <a
  //       href={`#${caseDocument.tabSafeId}`}
  //       onClick={(ev) => {
  //         handleOpenPdf(caseDocument);
  //       }}
  //       data-testid={`link-document-${caseDocument.documentId}`}
  //     >
  //       {caseDocument.presentationFileName}
  //     </a>
  //   </td>
  //   <td className={`govuk-table__cell govuk-body-s ${classes.date}`}>
  //     {caseDocument.cmsFileCreatedDate &&
  //       formatDate(
  //         caseDocument.cmsFileCreatedDate,
  //         CommonDateTimeFormats.ShortDateTextMonth
  //       )}
  //   </td>
  // </tr>
};
