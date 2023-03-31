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
  const { searchState } = restProps;

  const waitStatus = useMandatoryWaitPeriod(
    restProps.pipelineState.status === "complete" &&
      restProps.searchState.results.status === "succeeded",
    PAUSE_PERIOD_MS,
    MANDATORY_WAIT_PERIOD
  );

  if (waitStatus === "wait" && searchState.submittedSearchTerm) {
    return (
      <Modal
        isVisible={searchState.isResultsVisible}
        handleClose={handleCloseSearchResults}
      >
        <PleaseWait />
      </Modal>
    );
  } else {
    return (
      <Modal
        isVisible={searchState.isResultsVisible}
        handleClose={handleCloseSearchResults}
      >
        <Content {...restProps} />
      </Modal>
    );
  }
};
