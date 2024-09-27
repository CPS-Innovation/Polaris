import { useEffect } from "react";
import { useApi } from "../../../../common/hooks/useApi";
import { searchCase } from "../../api/gateway-api";
import { CombinedState } from "../../domain/CombinedState";
import { DispatchType } from "./reducer";

export const useDocumentSearch = (
  urn: string,
  caseId: number,
  combinedState: CombinedState,
  dispatch: DispatchType
) => {
  // Document search process
  const searchResults = useApi(
    searchCase,
    [
      urn,
      caseId,
      combinedState.searchState.submittedSearchTerm
        ? combinedState.searchState.submittedSearchTerm
        : "",
    ],
    //  Note: we let the user trigger a search without the pipeline being ready.
    //  If we additionally observe the complete-state of the pipeline here, we can ensure that a search
    //  is triggered when either:
    //  a) the pipeline is ready and the user subsequently submits a search
    //  b) the user submits a search before the pipeline is ready, but it then becomes ready
    // combinedState.pipelineState.status === "complete",
    //  It makes it much easier if we enforce that the documents need to be known before allowing
    //   a search (logically, we do not need to wait for the documents call to return at the point we trigger a
    //   search, we only need them when we map the eventual result of the search call).  However, this is a tidier
    //   place to enforce the wait as we are already waiting for the pipeline here. If we don't wait here, then
    //   we have to deal with the condition where the search results have come back but we do not yet have the
    //   the documents result, and we have to chase up fixing the full mapped objects at that later point.
    //   (Assumption: this is edge-casey stuff as the documents call should always really have come back unless
    //   the user is super quick to trigger a search).
    !!(
      combinedState.searchState.submittedSearchTerm &&
      combinedState.pipelineState.status === "complete" &&
      combinedState.documentsState.status === "succeeded"
    )
  );
  useEffect(() => {
    if (searchResults.status !== "initial") {
      dispatch({ type: "UPDATE_SEARCH_RESULTS", payload: searchResults });
    }
  }, [searchResults, dispatch]);
};
