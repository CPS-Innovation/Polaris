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
    const docsUploaded = state.documents.filter(
      (doc) => doc.status === "PdfUploadedToBlob" || doc.status === "Indexed"
    )?.length;
    const docFailed = state.documents.filter(
      (doc) =>
        doc.status === "UnableToConvertToPdf" ||
        doc.status === "UnexpectedFailure" ||
        doc.status === "New"
    )?.length;

    console.log("docFailed");
    if (isCaseCompleted) {
      return docFailed > 0
        ? `Documents ready to read - ${docsUploaded} (unable to convert [${docFailed}] ${
            docFailed > 1 ? "documents" : "document"
          })`
        : `Documents ready to read - ${docsUploaded}`;
    }
    return `Documents ready to read - ${docsUploaded}`;
  };

  const getDocumentsSearchIndexText = () => {
    if (isCaseCompleted) {
      const docsNotIndexed = state.documents.filter(
        (doc) => doc.status !== "Indexed"
      ).length;
      return docsNotIndexed
        ? `Search index creation complete  (unable to index [${docsNotIndexed}] ${
            docsNotIndexed > 1 ? "documents" : "document"
          })`
        : "Search index creation complete";
    }
    return `Search index creation in progress`;
  };

  const getCaseReadyText = () => {
    if (isCaseCompleted) return "Case is ready to search";
    return "Case is not ready to search";
  };

  return (
    // WARNING: this is used by the e2e tests to check the status of the documents
    <div
      data-testid={
        isCaseCompleted && everyDocIndexed ? "span-flag-all-indexed" : ""
      }
      className={classes.trackerSummary}
    >
      <span aria-live="polite">
        {`Total documents ${state.documents.length}`}{" "}
      </span>
      <span aria-live="polite">{getDocumentsReadyToReadText()}</span>
      <span aria-live="polite">{getDocumentsSearchIndexText()}</span>
      <span aria-live="polite">{getCaseReadyText()}</span>
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
