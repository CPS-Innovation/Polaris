import { PipelineDocument } from "../../../domain/gateway/PipelineDocument";
import { PipelineResults } from "../../../domain/gateway/PipelineResults";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
};

const zeroDocs = () => <>?</>;

const renderDocResults = (state: PipelineResults) => {
  const isCaseCompleted = state.status == "Completed";

  const everyDocIndexed = state.documents.every(
    (doc) => doc.status === "Indexed"
  );
  return (
    // WARNING: this is used by the e2e tests to check the status of the documents
    <span
      data-testid={
        isCaseCompleted && everyDocIndexed ? "span-flag-all-indexed" : ""
      }
      style={{ color: "#dddddd" }}
    >
      {state.documents.map((doc) => {
        return <span key={doc.documentId}>{renderDocResult(doc)}</span>;
      })}
    </span>
  );
};

const renderDocResult = (doc: PipelineDocument) => {
  switch (doc.status) {
    case "New":
      return <span>.</span>;
    case "PdfUploadedToBlob":
      return <span>-</span>;
    case "Indexed":
      return <span>+</span>;
    default:
      return <span style={{ color: "red" }}>x</span>;
  }
};

export const TrackerVisualisation: React.FC<Props> = ({ pipelineState }) => {
  if (!pipelineState.haveData) {
    return null;
  }

  return (
    <div style={{ textAlign: "center", fontFamily: "monospace" }}>
      {!pipelineState.data.documents.length
        ? zeroDocs()
        : renderDocResults(pipelineState.data)}
    </div>
  );
};
