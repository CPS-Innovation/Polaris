import { CombinedState } from "../../../../domain/CombinedState";
import { MappedTextSearchResult } from "../../../../domain/MappedTextSearchResult";
import { CaseDetailsState } from "../../../../hooks/use-case-details-state/useCaseDetailsState";
import { Filters } from "./Filters";
import { Header } from "./Header";
import { List } from "./List";
import { useAppInsightsTrackEvent } from "../../../../../../common/hooks/useAppInsightsTracks";

type Props = {
  submittedSearchTerm: string;
  requestedSearchTerm: string;
  searchResult: MappedTextSearchResult;
  missingDocs: CombinedState["searchState"]["missingDocs"];
  resultsOrder: CombinedState["searchState"]["searchConfigs"]["documentContent"]["resultsOrder"];
  previouslyIndexed: boolean;
  handleChangeResultsOrder: CaseDetailsState["handleChangeResultsOrder"];
  filterOptions: CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"];
  handleUpdateFilter: CaseDetailsState["handleUpdateFilter"];
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

export const Results: React.FC<Props> = ({
  searchResult,
  submittedSearchTerm,
  requestedSearchTerm,
  missingDocs,
  resultsOrder,
  previouslyIndexed,
  handleChangeResultsOrder,
  filterOptions,
  handleUpdateFilter,
  handleOpenPdf,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const updateFilterHandler = (payload: {
    filter: "docType" | "category";
    id: string;
    isSelected: boolean;
  }) => {
    trackEvent("Filter Doc Search Results", {
      filterCategory: payload.filter,
      filterId: payload.id,
      filterChecked: payload.isSelected,
      searchParameter: submittedSearchTerm,
    });
    handleUpdateFilter(payload);
  };
  return (
    <>
      <div className="govuk-grid-column-one-quarter">
        <Filters
          filterOptions={filterOptions}
          handleUpdateFilter={updateFilterHandler}
        />
      </div>
      <div className="govuk-grid-column-three-quarters">
        <Header
          {...{
            searchResult,
            submittedSearchTerm,
            requestedSearchTerm,
            missingDocs,
            resultsOrder,
            previouslyIndexed,
            handleChangeResultsOrder,
          }}
        />

        <List searchResult={searchResult} handleOpenPdf={handleOpenPdf} />
      </div>
    </>
  );
};
