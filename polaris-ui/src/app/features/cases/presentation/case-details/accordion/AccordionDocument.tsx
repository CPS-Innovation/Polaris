import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../common/utils/dates";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";

import classes from "./Accordion.module.scss";

type Props = {
  caseDocument: MappedCaseDocument;
  handleOpenPdf: (caseDocument: { documentId: number }) => void;
};

export const AccordionDocument: React.FC<Props> = ({
  caseDocument,
  handleOpenPdf,
}) => {
  return (
    <tr className="govuk-table__row">
      <td
        className={`govuk-table__cell govuk-body-s openMe ${classes["accordion-table__cell"]}`}
        style={{ wordWrap: "break-word" }}
      >
        <LinkButton
          onClick={() => {
            handleOpenPdf(caseDocument);
          }}
          className={`${classes["accordion-link-button"]}`}
          dataTestId={`link-document-${caseDocument.documentId}`}
        >
          {caseDocument.presentationFileName}
        </LinkButton>
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
