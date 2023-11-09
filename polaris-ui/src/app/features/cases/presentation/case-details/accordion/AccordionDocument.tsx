import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../common/utils/dates";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";

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
  const trackEvent = useAppInsightsTrackEvent();
  const canViewDocument = caseDocument.presentationFlags?.read === "Ok";
  const getAttachmentText = () => {
    if (caseDocument.attachments.length === 1) {
      return "1 attachment";
    }
    return `${caseDocument.attachments.length} attachments`;
  };

  return (
    <li className={`${classes["accordion-document-list-item"]}`}>
      <div className={`${classes["accordion-document-item-wrapper"]}`}>
        {canViewDocument ? (
          <LinkButton
            onClick={() => {
              trackEvent("Open Document From Case Details", {
                documentId: caseDocument.documentId,
              });
              handleOpenPdf({ documentId: caseDocument.documentId });
            }}
            className={`${classes["accordion-document-link-button"]}`}
            dataTestId={`link-document-${caseDocument.documentId}`}
            ariaLabel={`Open Document ${caseDocument.presentationFileName}`}
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
          <span className={`${classes["visuallyHidden"]}`}> Date Added</span>
          {caseDocument.cmsFileCreatedDate &&
            formatDate(
              caseDocument.cmsFileCreatedDate,
              CommonDateTimeFormats.ShortDateTextMonth
            )}
        </span>
        {!!caseDocument.attachments.length && (
          <span>{getAttachmentText()}</span>
        )}
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
