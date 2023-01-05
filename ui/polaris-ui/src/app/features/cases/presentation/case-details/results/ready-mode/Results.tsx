import { CombinedState } from "../../../../domain/CombinedState";
import { MappedTextSearchResult } from "../../../../domain/MappedTextSearchResult";
import { CaseDetailsState } from "../../../../hooks/use-case-details-state/useCaseDetailsState";
import { Filters } from "./Filters";
import { Header } from "./Header";
import { List } from "./List";

type Props = {
  submittedSearchTerm: string;
  requestedSearchTerm: string;
  searchResult: MappedTextSearchResult;
  missingDocs: CombinedState["searchState"]["missingDocs"];
  resultsOrder: CaseDetailsState["searchState"]["resultsOrder"];
  handleChangeResultsOrder: CaseDetailsState["handleChangeResultsOrder"];
  filterOptions: CombinedState["searchState"]["filterOptions"];
  handleUpdateFilter: CaseDetailsState["handleUpdateFilter"];
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

export const Results: React.FC<Props> = ({
  searchResult,
  submittedSearchTerm,
  requestedSearchTerm,
  missingDocs,
  resultsOrder,
  handleChangeResultsOrder,
  filterOptions,
  handleUpdateFilter,
  handleOpenPdf,
}) => {
  return (
    <>
      <div className="govuk-grid-column-one-quarter">
        <Filters
          filterOptions={filterOptions}
          handleUpdateFilter={handleUpdateFilter}
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
            handleChangeResultsOrder,
          }}
        />

        <List searchResult={searchResult} handleOpenPdf={handleOpenPdf} />
      </div>
    </>
  );
};
