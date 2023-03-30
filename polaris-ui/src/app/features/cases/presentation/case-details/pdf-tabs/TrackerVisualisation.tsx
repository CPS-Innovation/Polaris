import { PipelineDocument } from "../../../domain/PipelineDocument";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
};

const zeroDocs = () => <>?</>;

const renderDocResults = (docs: PipelineDocument[]) => {
  return docs.map((doc) => {
    return <span key={doc.documentId}>{renderDocResult(doc)}</span>;
  });
};

const renderDocResult = (doc: PipelineDocument) => {
  switch (doc.status) {
    case "New":
      return <span style={{ color: "#dddddd" }}>.</span>;
    case "PdfUploadedToBlob":
      return <span style={{ color: "#dddddd" }}>-</span>;
    case "Indexed":
      return <span style={{ color: "#dddddd" }}>+</span>;
    default:
      return <span style={{ color: "#dddddd" }}>x</span>;
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
        : renderDocResults(pipelineState.data.documents)}
    </div>
  );
};
