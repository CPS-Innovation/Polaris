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
    if (isCaseCompleted && docFailed) {
      return `Documents ready to read: ${docsUploaded} (unable to convert ${docFailed} ${
        docFailed > 1 ? "documents" : "document"
      })`;
    }
    return `Documents ready to read: ${docsUploaded}`;
  };

  const getDocumentsSearchIndexText = () => {
    const docsNotIndexed = validDocs.filter(
      (doc) => doc.status !== "Indexed"
    ).length;
    const docsIndexed = validDocs.length - docsNotIndexed;
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
    // WARNING: "span-flag-all-indexed" is used by the e2e tests to check the status of the documents
    <div data-testid="tracker-summary" className={classes.trackerSummary}>
      <span aria-live="polite">{`Total documents: ${validDocs.length}`}</span>
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

export const TrackerSummary: React.FC<Props> = ({
  pipelineState,
  isMultipleDefendantsOrCharges,
}) => {
  // remove the filter out of DAC logic(validDocs) when the BUG 27712 is resolved
  const validDocs = useMemo(() => {
    if (!pipelineState.haveData) {
      return [];
    }
    return isMultipleDefendantsOrCharges
      ? pipelineState.data.documents
      : pipelineState.data.documents.filter(
          (doc) => doc.cmsDocType.documentType !== "DAC"
        );
  }, [pipelineState, isMultipleDefendantsOrCharges]);

  if (!pipelineState.haveData || !validDocs.length) {
    return null;
  }

  return <div>{renderDocResults(pipelineState.data.status, validDocs)}</div>;
};
