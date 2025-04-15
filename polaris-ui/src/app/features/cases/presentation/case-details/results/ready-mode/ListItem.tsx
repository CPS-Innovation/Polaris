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
import { useAppInsightsTrackEvent } from "../../../../../../common/hooks/useAppInsightsTracks";
import classes from "./ListItem.module.scss";
import { CombinedState } from "../../../../domain/CombinedState";
import { FeatureFlagData } from "../../../../domain/FeatureFlagData";

type Props = {
  documentResult: MappedDocumentResult;
  submittedSearchTerm: CombinedState["searchState"]["submittedSearchTerm"];
  featureFlags: FeatureFlagData;
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

export const ListItem: React.FC<Props> = ({
  documentResult: {
    presentationTitle,
    documentId,
    cmsFileCreatedDate: createdDate,
    cmsDocType,
    isDocumentNameMatch,
    occurrences,
    occurrencesInDocumentCount,
  },
  submittedSearchTerm,
  featureFlags,
  handleOpenPdf,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [firstOccurrence, ...subsequentOccurrences] = occurrences;
  const shouldShowDocumentNameMatches = isDocumentNameMatch && featureFlags.documentNameSearch

  return (
    <div data-testid={`div-search-result-${documentId}`}>
      <LinkButton
        onClick={() => {
          trackEvent("Open Document From Document Search", {
            documentId: documentId,
          });
          handleOpenPdf({ documentId, mode: "search" });
        }}
        ariaLabel={`Open Document ${presentationTitle}`}
        dataTestId={`link-result-document-${documentId}`}
        className={classes.headingLinkButton}
      >
        {presentationTitle}
      </LinkButton>

      <div className="govuk-body-s">
        <div>
          <span className={classes.label}>Uploaded:</span>
          {formatDate(createdDate, CommonDateTimeFormats.ShortDateTextMonth)}
        </div>
        <div>
          <span className={classes.label}>Type:</span>
          {cmsDocType.documentType}
        </div>
      </div>

      {shouldShowDocumentNameMatches && (
        <div className="govuk-details__text">
          <span>Filename contains <b>{submittedSearchTerm}</b></span>
        </div>
      )}

      {!(isDocumentNameMatch && featureFlags.documentNameSearch) && firstOccurrence && (
        <ContextText contextTextChunks={firstOccurrence.contextTextChunks} />
      )}

      {(shouldShowDocumentNameMatches && occurrences.length) || subsequentOccurrences.length ? (
        <Details
          data-testid="details-expand-search-results"
          isDefaultLeftBorderHidden
          onClick={() => {
            trackEvent("View 'x' More", {
              viewMoreCount:
                occurrencesInDocumentCount -
                firstOccurrence.occurrencesInLine.length +
                (shouldShowDocumentNameMatches ? 1 : 0)
              ,
            });
          }}
          summaryChildren={`View ${occurrencesInDocumentCount -
            firstOccurrence.occurrencesInLine.length +
            (shouldShowDocumentNameMatches ? 1 : 0)
            } more`}
          children={shouldShowDocumentNameMatches ?
            occurrences.map((occurrence) => (
              <span key={occurrence.id}>
                <ContextText contextTextChunks={occurrence.contextTextChunks} />
              </span>
            )) :
            subsequentOccurrences.map((occurrence) => (
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
