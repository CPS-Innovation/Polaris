import { useMemo, useState, useEffect } from "react";
import { Modal } from "../../../../../common/presentation/components";
import { SucceededApiResult } from "../../../../../common/types/SucceededApiResult";
import { CaseDetails } from "../../../domain/gateway/CaseDetails";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { Content } from "./Content";
import { useMandatoryWaitPeriod } from "../../../hooks/useMandatoryWaitPeriod";
import { PleaseWait } from "./PleaseWait";

type Props = {
  // This is intentionally narrower than ApiResult<...> as we definitely have
  //  the Api result and means we do not have to check for the "loading" case
  //  from here onwards.
  caseState: SucceededApiResult<CaseDetails>;
  searchTerm: CaseDetailsState["searchTerm"];
  searchState: CaseDetailsState["searchState"];
  pipelineState: CaseDetailsState["pipelineState"];
  handleSearchTermChange: CaseDetailsState["handleSearchTermChange"];
  handleCloseSearchResults: CaseDetailsState["handleCloseSearchResults"];
  handleLaunchSearchResults: CaseDetailsState["handleLaunchSearchResults"];
  handleChangeResultsOrder: CaseDetailsState["handleChangeResultsOrder"];
  handleUpdateFilter: CaseDetailsState["handleUpdateFilter"];
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

// make PAUSE_PERIOD_MS > 0 to allow a window of time where results can appear without he Please Wait
//  appearing if the data is available quicker than PAUSE_PERIOD_MS
const PAUSE_PERIOD_MS = 0;
const MANDATORY_WAIT_PERIOD = 600;

export const ResultsModal: React.FC<Props> = ({
  handleCloseSearchResults,
  ...restProps
}) => {
  //this is just to show the loading percentage only when the first pipeline refresh.
  const [showLoadingPercentage, setShowLoadingPercentage] = useState(
    !restProps.pipelineState?.data
  );

  const { searchState } = restProps;
  const percentageCompleted = useMemo(() => {
    const docs = restProps.pipelineState.data
      ? restProps.pipelineState.data.documents
      : [];
    const indexedDocs = docs.filter((doc) => doc.status === "Indexed");
    if (!docs.length) return 0;
    return Math.round(indexedDocs.length / docs.length) * 100;
  }, [restProps.pipelineState]);

  const waitStatus = useMandatoryWaitPeriod(
    restProps.pipelineState.status === "complete" &&
      restProps.searchState.results.status === "succeeded",
    PAUSE_PERIOD_MS,
    MANDATORY_WAIT_PERIOD
  );

  useEffect(() => {
    if (
      showLoadingPercentage &&
      waitStatus !== "wait" &&
      restProps.pipelineState?.data?.status === "Completed"
    ) {
      setShowLoadingPercentage(false);
    }
  }, [
    restProps.pipelineState?.data?.status,
    showLoadingPercentage,
    setShowLoadingPercentage,
    waitStatus,
  ]);
  return (
    <Modal
      isVisible={searchState.isResultsVisible}
      handleClose={handleCloseSearchResults}
      ariaLabel="Search Modal"
      ariaDescription="Find your search results"
    >
      {waitStatus === "wait" &&
      searchState.submittedSearchTerm !==
        searchState.lastSubmittedSearchTerm ? (
        <PleaseWait
          percentageCompleted={percentageCompleted}
          showLoadingPercentage={showLoadingPercentage}
        />
      ) : (
        <Content {...restProps} />
      )}
    </Modal>
  );
};
