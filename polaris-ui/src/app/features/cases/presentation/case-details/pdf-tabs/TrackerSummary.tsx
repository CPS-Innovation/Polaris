import { PipelineResults } from "../../../domain/gateway/PipelineResults";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import classes from "./TrackerSummary.module.scss";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
};

const renderDocResults = (state: PipelineResults) => {
  const isCaseCompleted = state.status === "Completed";

  const everyDocIndexed = state.documents.every(
    (doc) => doc.status === "Indexed"
  );

  const getDocumentsReadyToReadText = () => {
    const docFailed = state.documents.filter(
      (doc) =>
        doc.status === "UnableToConvertToPdf" ||
        doc.status === "UnexpectedFailure" ||
        doc.status === "New"
    )?.length;
    const docsUploaded = state.documents.length - docFailed;
    if (isCaseCompleted && docFailed) {
      return `Documents ready to read: ${docsUploaded} (unable to convert ${docFailed} ${
        docFailed > 1 ? "documents" : "document"
      })`;
    }
    return `Documents ready to read: ${docsUploaded}`;
  };

  const getDocumentsSearchIndexText = () => {
    const docsNotIndexed = state.documents.filter(
      (doc) => doc.status !== "Indexed"
    ).length;
    const docsIndexed = state.documents.length - docsNotIndexed;
    if (isCaseCompleted && docsNotIndexed) {
      return `Documents indexed: ${docsIndexed} (unable to index ${docsNotIndexed} ${
        docsNotIndexed > 1 ? "documents" : "document"
      })`;
    }
    return `Documents indexed: ${docsIndexed}`;
  };

  const getCaseSearchReadyText = () => {
    if (isCaseCompleted) return "Case is ready to search";
    return "Case is not ready to search";
  };

  return (
    // WARNING: this is used by the e2e tests to check the status of the documents
    <div data-testid="tracker-summary" className={classes.trackerSummary}>
      <span aria-live="polite">
        {`Total documents: ${state.documents.length}`}
      </span>
      <span aria-live="polite">{getDocumentsReadyToReadText()}</span>
      <span aria-live="polite">{getDocumentsSearchIndexText()}</span>
      <span
        aria-live="polite"
        data-testid={
          isCaseCompleted && everyDocIndexed ? "span-flag-all-indexed" : ""
        }
      >
        {getCaseSearchReadyText()}
      </span>
    </div>
  );
};

export const TrackerSummary: React.FC<Props> = ({ pipelineState }) => {
  if (!pipelineState.haveData) {
    return null;
  }

  return (
    <div>
      {!!pipelineState.data.documents.length &&
        renderDocResults(pipelineState.data)}
    </div>
  );
};
