import { useState, useEffect } from "react";
import { Modal } from "../../../../../common/presentation/components";
import { SucceededApiResult } from "../../../../../common/types/SucceededApiResult";
import { CaseDetails } from "../../../domain/gateway/CaseDetails";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { Content } from "./Content";
import { useMandatoryWaitPeriod } from "../../../hooks/useMandatoryWaitPeriod";
import { CombinedState } from "../../../domain/CombinedState";
import { PleaseWait } from "./PleaseWait";
import { TrackerSummary } from "./TrackerSummary";
import classes from "./ResultsModal.module.scss";

type Props = {
  // This is intentionally narrower than ApiResult<...> as we definitely have
  //  the Api result and means we do not have to check for the "loading" case
  //  from here onwards.
  caseState: SucceededApiResult<CaseDetails>;
  searchTerm: CombinedState["searchTerm"];
  searchState: CombinedState["searchState"];
  pipelineState: CombinedState["pipelineState"];
  featureFlags: CombinedState["featureFlags"];
  handleSearchTermChange: CaseDetailsState["handleSearchTermChange"];
  handleSearchTypeChange: CaseDetailsState["handleSearchTypeChange"];
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
  const [showLoadingPercentage, setShowLoadingPercentage] = useState(false);
  const [loadingPercentage, setLoadingPercentage] = useState(0);

  const { searchState } = restProps;
  useEffect(() => {
    const docs = restProps.pipelineState.data
      ? restProps.pipelineState.data.documents
      : [];
    const indexedDocs = docs.filter((doc) => doc.status === "Indexed");
    const percentage = !docs.length
      ? 0
      : Math.round((indexedDocs.length / docs.length) * 100);

    setLoadingPercentage(percentage);
  }, [restProps.pipelineState]);

  const waitStatus = useMandatoryWaitPeriod(
    restProps.pipelineState.status === "complete" &&
      restProps.searchState.searchConfigs.documentContent.results.status ===
        "succeeded",
    PAUSE_PERIOD_MS,
    MANDATORY_WAIT_PERIOD
  );

  useEffect(() => {
    if (
      showLoadingPercentage &&
      waitStatus !== "wait" &&
      restProps.pipelineState?.data?.status === "Completed"
    ) {
      const timeout = setTimeout(() => {
        setShowLoadingPercentage(false);
      }, 500);

      return () => {
        clearTimeout(timeout);
      };
    }
    if (
      waitStatus === "wait" &&
      !showLoadingPercentage &&
      restProps.pipelineState?.data &&
      restProps.pipelineState?.data?.status !== "Completed"
    ) {
      setLoadingPercentage(0);
      setShowLoadingPercentage(true);
    }
  }, [restProps.pipelineState?.data, showLoadingPercentage, waitStatus]);
  return (
    <Modal
      isVisible={searchState.isResultsVisible}
      handleClose={handleCloseSearchResults}
      ariaLabel="Search Modal"
      ariaDescription="Find your search results"
    >
      {restProps.featureFlags.documentNameSearch ? (
        <Content {...restProps} loadingPercentage={loadingPercentage} />
      ) : (
        <>
          {waitStatus === "wait" &&
          searchState.submittedSearchTerm !==
            searchState.lastSubmittedSearchTerm ? (
            <div className={classes.loadingContent}>
              <PleaseWait
                percentageCompleted={loadingPercentage}
                showLoadingPercentage={showLoadingPercentage}
              />

              {showLoadingPercentage && (
                <TrackerSummary pipelineState={restProps.pipelineState} />
              )}
            </div>
          ) : (
            <Content {...restProps} />
          )}
        </>
      )}
    </Modal>
  );
};
