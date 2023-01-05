import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../common/utils/dates";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";

import classes from "./Accordion.module.scss";

type Props = {
  caseDocument: MappedCaseDocument;
  handleOpenPdf: (caseDocument: {
    tabSafeId: string;
    documentId: number;
  }) => void;
};

export const AccordionDocument: React.FC<Props> = ({
  caseDocument,
  handleOpenPdf,
}) => {
  return (
    <tr className="govuk-table__row">
      <td
        className="govuk-table__cell govuk-body-s openMe"
        style={{ wordWrap: "break-word" }}
      >
        <a
          href={`#${caseDocument.tabSafeId}`}
          onClick={(ev) => {
            handleOpenPdf(caseDocument);
          }}
          data-testid={`link-document-${caseDocument.documentId}`}
        >
          {caseDocument.presentationFileName}
        </a>
      </td>
      <td className={`govuk-table__cell govuk-body-s ${classes.date}`}>
        {caseDocument.createdDate &&
          formatDate(
            caseDocument.createdDate,
            CommonDateTimeFormats.ShortDateTextMonth
          )}
      </td>
    </tr>
  );
};
