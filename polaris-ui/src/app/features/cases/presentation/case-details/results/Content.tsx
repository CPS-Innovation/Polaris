import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { SearchBox } from "../search-box/SearchBox";
import classes from "./Content.module.scss";
import { SucceededApiResult } from "../../../../../common/types/SucceededApiResult";
import { CaseDetails } from "../../../domain/gateway/CaseDetails";
import { Results } from "./ready-mode/Results";
import { CombinedState } from "../../../domain/CombinedState";
import React, { useEffect, useState } from "react";
import {
  LinkButton,
  NotificationBanner,
} from "../../../../../common/presentation/components";
import { FeatureFlagData } from "../../../domain/FeatureFlagData";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";

type Props = {
  caseState: SucceededApiResult<CaseDetails>;
  searchTerm: CombinedState["searchTerm"];
  searchState: CombinedState["searchState"];
  loadingPercentage?: number;
  featureFlags: FeatureFlagData;
  handleSearchTermChange: CaseDetailsState["handleSearchTermChange"];
  handleSearchTypeChange: CaseDetailsState["handleSearchTypeChange"];
  handleLaunchSearchResults: CaseDetailsState["handleLaunchSearchResults"];
  handleChangeResultsOrder: CaseDetailsState["handleChangeResultsOrder"];
  handleUpdateFilter: CaseDetailsState["handleUpdateFilter"];
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

const MemoizedResults = React.memo(Results);

export const Content: React.FC<Props> = ({
  caseState: {
    data: { leadDefendantDetails, uniqueReferenceNumber },
  },
  searchTerm: value,
  searchState: {
    submittedSearchTerm,
    requestedSearchTerm,
    lastSubmittedSearchTerm,
    searchConfigs: { documentName, documentContent },
    missingDocs,
  },
  loadingPercentage,
  featureFlags,
  handleSearchTermChange: handleChange,
  handleSearchTypeChange,
  handleLaunchSearchResults: handleSubmit,
  handleChangeResultsOrder,
  handleUpdateFilter,
  handleOpenPdf,
}) => {
  const trackEvent = useAppInsightsTrackEvent();

  const [previouslyIndexed, setPreviouslyIndexed] = useState(
    (documentContent.results.status === "succeeded" &&
      submittedSearchTerm === lastSubmittedSearchTerm) ||
      !featureFlags.documentNameSearch
  );

  useEffect(() => {
    if (documentContent.results.status === "succeeded") {
      handleSearchTypeChange("documentContent");
    }
  }, []);

  const handleUpdateResults = () => {
    trackEvent("Search Results Available Link");
    handleSearchTypeChange("documentContent");
    setPreviouslyIndexed(true);
  };

  const handleResetSearch = (currentValue: string) => {
    if (requestedSearchTerm !== currentValue) {
      setPreviouslyIndexed(false);
      handleSubmit();
    }
  };

  const labelText = leadDefendantDetails
    ? `Search ${leadDefendantDetails.surname}, ${uniqueReferenceNumber}`
    : `Search ${uniqueReferenceNumber}`;

  return (
    <div
      className={`govuk-width-container ${classes.content}`}
      data-testid="div-search-results"
    >
      {featureFlags.documentNameSearch && (
        <div className={classes.notificationContainer}>
          {submittedSearchTerm && requestedSearchTerm && !previouslyIndexed && (
            <>
              {documentContent.results.status === "loading" ? (
                <NotificationBanner className={classes.notificationBanner}>
                  <div
                    className={classes.bannerContent}
                    data-testid="div-notification-information-banner"
                  >
                    <p className={classes.notificationBannerHeading}>
                      The full search results are being prepared -{" "}
                      {loadingPercentage}% complete
                    </p>
                    <p>
                      In the meantime search results on material filenames are
                      displayed below.
                    </p>
                  </div>
                </NotificationBanner>
              ) : null}
              {documentContent.results.status === "succeeded" ? (
                <NotificationBanner {...{ type: "success" }}>
                  <div
                    className={classes.bannerContent}
                    data-testid="div-notification-success-banner"
                  >
                    <p className={classes.notificationBannerHeading}>
                      The full search results are now available -
                      <LinkButton
                        onClick={handleUpdateResults}
                        ariaLabel={"Search Results Available Link"}
                        dataTestId={"search-results-available-link"}
                        className={classes.searchResultsAvailableLink}
                      >
                        update this page
                      </LinkButton>
                    </p>
                  </div>
                </NotificationBanner>
              ) : null}
            </>
          )}
        </div>
      )}

      <div className="govuk-grid-row">
        <div className="govuk-!-width-one-half">
          <SearchBox
            {...{
              labelText,
              value,
              handleChange,
              handleSubmit: () => handleResetSearch(value),
            }}
            data-testid="results-search-case"
            id="case-details-result-search"
            trackEventKey="Search Case Documents From Document Search"
          />
        </div>
      </div>

      <div className="govuk-grid-row">
        {submittedSearchTerm && requestedSearchTerm && (
          <>
            {previouslyIndexed &&
            documentContent.results.status === "succeeded" ? (
              <MemoizedResults
                {...{
                  missingDocs,
                  searchResult: documentContent.results.data,
                  submittedSearchTerm,
                  requestedSearchTerm,
                  resultsOrder: documentContent.resultsOrder,
                  filterOptions: documentContent.filterOptions,
                  previouslyIndexed,
                  featureFlags,
                  handleChangeResultsOrder,
                  handleUpdateFilter,
                  handleOpenPdf,
                }}
              />
            ) : (
              <>
                {documentName.results.status === "succeeded" && (
                  <MemoizedResults
                    {...{
                      missingDocs: [],
                      searchResult: documentName.results.data,
                      submittedSearchTerm,
                      requestedSearchTerm,
                      resultsOrder: documentName.resultsOrder,
                      filterOptions: documentName.filterOptions,
                      previouslyIndexed,
                      featureFlags,
                      handleChangeResultsOrder,
                      handleUpdateFilter,
                      handleOpenPdf,
                    }}
                  />
                )}
              </>
            )}
          </>
        )}
        <div>
          {!submittedSearchTerm && !requestedSearchTerm && (
            <p> Please enter your search term.</p>
          )}
        </div>
      </div>
    </div>
  );
};
