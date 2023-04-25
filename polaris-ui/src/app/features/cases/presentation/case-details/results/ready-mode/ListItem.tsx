import { Details } from "../../../../../../common/presentation/components";
import { SectionBreak } from "../../../../../../common/presentation/components";
import { LinkButton } from "../../../../../../common/presentation/components/LinkButton";
import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../../common/utils/dates";
import { MappedDocumentResult } from "../../../../domain/MappedDocumentResult";
import { CaseDetailsState } from "../../../../hooks/use-case-details-state/useCaseDetailsState";
import { ContextText } from "./ContextText";
import { useAppInsightsTrackEvent } from "../../../../../../common/hooks/useAppInsightTrackEvent";
import classes from "./ListItem.module.scss";

type Props = {
  documentResult: MappedDocumentResult;
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

export const ListItem: React.FC<Props> = ({
  documentResult: {
    presentationFileName,
    documentId,
    cmsFileCreatedDate: createdDate,
    cmsDocType,
    occurrences: [firstOcurrence, ...subsequentOccurrences],
    occurrencesInDocumentCount,
  },
  handleOpenPdf,
}) => {
  const { trackEvent } = useAppInsightsTrackEvent();
  return (
    <div data-testid={`div-search-result-${documentId}`}>
      <h2 className="govuk-heading-s results-header">
        <LinkButton
          onClick={() => {
            trackEvent("Open Document From Document Search", {
              documentId: documentId,
              presentationFileName: presentationFileName,
            });
            handleOpenPdf({ documentId, mode: "search" });
          }}
          dataTestId={`link-result-document-${documentId}`}
          className={classes.headingLinkButton}
        >
          {presentationFileName}
        </LinkButton>
      </h2>

      <div className="govuk-body-s">
        <div>
          <span className={classes.label}>Uploaded:</span>
          {formatDate(createdDate, CommonDateTimeFormats.ShortDateTextMonth)}
        </div>
        <div>
          <span className={classes.label}>Type:</span>
          {cmsDocType.name}
        </div>
      </div>

      <ContextText contextTextChunks={firstOcurrence.contextTextChunks} />

      {subsequentOccurrences.length ? (
        <Details
          data-testid="details-expand-search-results"
          isDefaultLeftBorderHidden
          onClick={() => {
            trackEvent("View 'x' More");
          }}
          summaryChildren={`View ${
            occurrencesInDocumentCount - firstOcurrence.occurrencesInLine.length
          } more`}
          children={subsequentOccurrences.map((occurrence) => (
            <span key={occurrence.id}>
              <ContextText contextTextChunks={occurrence.contextTextChunks} />
            </span>
          ))}
        />
      ) : null}

      <SectionBreak />
    </div>
  );
};
