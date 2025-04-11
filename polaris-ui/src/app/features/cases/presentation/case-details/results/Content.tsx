import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { SearchBox } from "../search-box/SearchBox";
import classes from "./Content.module.scss";
import { SucceededApiResult } from "../../../../../common/types/SucceededApiResult";
import { CaseDetails } from "../../../domain/gateway/CaseDetails";
import { Results } from "./ready-mode/Results";
import { CombinedState } from "../../../domain/CombinedState";
import React, { useState } from "react";
import { LinkButton, NotificationBanner } from "../../../../../common/presentation/components";

type Props = {
  caseState: SucceededApiResult<CaseDetails>;
  searchTerm: CombinedState["searchTerm"];
  searchState: CombinedState["searchState"];
  loadingPercentage?: number;
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
    searchConfigs: {
      DocumentName,
      DocumentContent
    },
    missingDocs,
  },
  loadingPercentage,
  handleSearchTermChange: handleChange,
  handleSearchTypeChange,
  handleLaunchSearchResults: handleSubmit,
  handleChangeResultsOrder,
  handleUpdateFilter,
  handleOpenPdf,
}) => {
  const [previouslyIndexed, setPreviouslyIndexed] = useState(DocumentContent.results.status === 'succeeded' && submittedSearchTerm ===
    lastSubmittedSearchTerm);

  const handleRefresh = () => {
    handleSearchTypeChange("DocumentContent");
    setPreviouslyIndexed(true);
  };

  const labelText = leadDefendantDetails
    ? `Search ${leadDefendantDetails.surname}, ${uniqueReferenceNumber}`
    : `Search ${uniqueReferenceNumber}`;

  return (
    <div
      className={`govuk-width-container ${classes.content}`}
      data-testid="div-search-results"
    >
      <div className={classes.notificationContainer}>
        {submittedSearchTerm && requestedSearchTerm && !previouslyIndexed && (
          <>
            {DocumentContent.results.status === "loading" ? (
              <NotificationBanner className={classes.notificationBanner}>
                <div
                  className={classes.bannerContent}
                  data-testid="div-notification-information-banner"
                >
                  <p className={classes.notificationBannerHeading}>
                    The full search results are being prepared - {loadingPercentage}% complete
                  </p>
                  <p>In the meantime search results on material filenames are displayed below.</p>
                </div>
              </NotificationBanner>
            ) : null}
            {DocumentContent.results.status === "succeeded" ? (
              <NotificationBanner {...{ type: 'success' }}>
                <div
                  className={classes.bannerContent}
                  data-testid="div-notification-success-banner"
                >
                  <p className={classes.notificationBannerHeading}>
                    The full search results are now available -
                    <LinkButton
                      onClick={handleRefresh}
                      ariaLabel={'Search Results Available Link'}
                      dataTestId={'search-results-available-link'}
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

      <div className="govuk-grid-row">
        <div className="govuk-!-width-one-half">
          <SearchBox
            {...{ labelText, value, handleChange, handleSubmit }}
            data-testid="results-search-case"
            id="case-details-result-search"
            trackEventKey="Search Case Documents From Document Search"
          />
        </div>
      </div>

      <div className="govuk-grid-row">
        {submittedSearchTerm &&
          requestedSearchTerm && (
            <>
              {previouslyIndexed && DocumentContent.results.status === "succeeded" ? (
                <MemoizedResults
                  {...{
                    missingDocs,
                    searchResult: DocumentContent.results.data,
                    submittedSearchTerm,
                    requestedSearchTerm,
                    resultsOrder: DocumentContent.resultsOrder,
                    filterOptions: DocumentContent.filterOptions,
                    previouslyIndexed,
                    handleChangeResultsOrder,
                    handleUpdateFilter,
                    handleOpenPdf,
                  }}
                />
              ) : (
                <>
                  {
                    DocumentName.results.status === "succeeded" && (
                      <MemoizedResults
                        {...{
                          missingDocs: [],
                          searchResult: DocumentName.results.data,
                          submittedSearchTerm,
                          requestedSearchTerm,
                          resultsOrder: DocumentName.resultsOrder,
                          filterOptions: DocumentName.filterOptions,
                          previouslyIndexed,
                          handleChangeResultsOrder,
                          handleUpdateFilter,
                          handleOpenPdf,
                        }}
                      />
                    )
                  }
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
    </div >
  );
};
