import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { SearchBox } from "../search-box/SearchBox";
import classes from "./Content.module.scss";
import { SucceededApiResult } from "../../../../../common/types/SucceededApiResult";
import { CaseDetails } from "../../../domain/gateway/CaseDetails";
import { Results } from "./ready-mode/Results";
import React from "react";

type Props = {
  caseState: SucceededApiResult<CaseDetails>;
  searchTerm: CaseDetailsState["searchTerm"];
  searchState: CaseDetailsState["searchState"];
  handleSearchTermChange: CaseDetailsState["handleSearchTermChange"];
  handleLaunchSearchResults: CaseDetailsState["handleLaunchSearchResults"];
  handleChangeResultsOrder: CaseDetailsState["handleChangeResultsOrder"];
  handleUpdateFilter: CaseDetailsState["handleUpdateFilter"];
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

const MemoizedResults = React.memo(Results);

export const Content: React.FC<Props> = ({
  caseState: {
    data: {
      leadDefendantDetails: { surname },
      uniqueReferenceNumber,
    },
  },
  searchTerm: value,
  searchState: {
    results,
    submittedSearchTerm,
    requestedSearchTerm,
    missingDocs,
    resultsOrder,
    filterOptions,
  },
  handleSearchTermChange: handleChange,
  handleLaunchSearchResults: handleSubmit,
  handleChangeResultsOrder,
  handleUpdateFilter,
  handleOpenPdf,
}) => {
  const labelText = `Search ${surname}, ${uniqueReferenceNumber}`;

  return (
    <div
      className={`govuk-width-container ${classes.content}`}
      data-testid="div-search-results"
    >
      <div className="govuk-grid-row">
        <div className="govuk-!-width-one-half">
          <SearchBox
            {...{ labelText, value, handleChange, handleSubmit }}
            data-testid="results-search-case"
            id="case-details-result-search"
          />
        </div>
      </div>

      <div className="govuk-grid-row">
        {submittedSearchTerm &&
        requestedSearchTerm &&
        results.status === "succeeded" ? (
          <MemoizedResults
            {...{
              missingDocs,
              searchResult: results.data,
              submittedSearchTerm,
              requestedSearchTerm,
              resultsOrder,
              filterOptions,
              handleChangeResultsOrder,
              handleUpdateFilter,
              handleOpenPdf,
            }}
          />
        ) : null}
        <div>
          {!submittedSearchTerm && !requestedSearchTerm && (
            <p> Please enter your search term.</p>
          )}
        </div>
      </div>
    </div>
  );
};
