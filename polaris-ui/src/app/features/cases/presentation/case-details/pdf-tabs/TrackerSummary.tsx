import { useMemo } from "react";
import { PipelineResults } from "../../../domain/gateway/PipelineResults";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import classes from "./TrackerSummary.module.scss";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
  isMultipleDefendantsOrCharges: boolean;
};

const renderDocResults = (
  status: PipelineResults["status"],
  validDocs: PipelineResults["documents"]
) => {
  const isCaseCompleted = status === "Completed";
  const everyDocIndexed = validDocs.every((doc) => doc.status === "Indexed");

  const getDocumentsReadyToReadText = () => {
    const docFailed = validDocs.filter(
      (doc) =>
        doc.status === "UnableToConvertToPdf" ||
        doc.status === "UnexpectedFailure" ||
        doc.status === "New"
    )?.length;
    const docsUploaded = validDocs.length - docFailed;
    const documentText = docFailed > 1 ? "documents" : "document";
    return (
      <span>
        Documents ready to read:{" "}
        <span className={classes.documentsReadyToReadText}>
          {isCaseCompleted && docFailed ? (
            `${docsUploaded} (unable to convert ${docFailed} ${documentText})`
          ) : (
            <>{docsUploaded}</>
          )}
        </span>
      </span>
    );
  };

  const getDocumentsSearchIndexText = () => {
    const docsNotIndexed = validDocs.filter(
      (doc) => doc.status !== "Indexed"
    ).length;
    const docsIndexed = validDocs.length - docsNotIndexed;
    const documentText = docsNotIndexed > 1 ? "documents" : "document";
    return (
      <span>
        Documents indexed:{" "}
        <span className={classes.documentsSearchIndexText}>
          {isCaseCompleted && docsNotIndexed ? (
            `${docsIndexed} (unable to index ${docsNotIndexed} ${documentText})`
          ) : (
            <>{docsIndexed}</>
          )}
        </span>
      </span>
    );
  };

  const getCaseSearchReadyText = () => {
    if (isCaseCompleted) return "Case is ready to search";
    return "Case is not ready to search";
  };

  return (
    // WARNING: "span-flag-all-indexed" is used by the e2e tests to check the status of the documents
    <div data-testid="tracker-summary" className={classes.trackerSummary}>
      <span aria-live="polite">
        Total documents:{" "}
        <span className={classes.totalDocuments}>{validDocs.length}</span>
      </span>
      <span aria-live="polite">{getDocumentsReadyToReadText()}</span>
      <span aria-live="polite">{getDocumentsSearchIndexText()}</span>
      <span
        aria-live="polite"
        //this is needed for polaris-e2e test
        data-testid={
          isCaseCompleted && everyDocIndexed ? "span-flag-all-indexed" : ""
        }
        className={classes.caseSearchReadyText}
      >
        {getCaseSearchReadyText()}
      </span>
    </div>
  );
};
export const TrackerSummary: React.FC<Props> = ({ pipelineState }) => {
  if (!pipelineState?.data) {
    return null;
  }

  return (
    <div>
      {renderDocResults(
        pipelineState.data.status,
        pipelineState.data.documents
      )}
    </div>
  );
};
