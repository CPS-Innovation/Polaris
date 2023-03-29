import { PipelineDocument } from "../../../domain/PipelineDocument";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
};

const renderDoc = (doc: PipelineDocument) => {
  switch (doc.status) {
    case "New":
      return <span style={{ color: "lightgrey" }}>.</span>;
    case "PdfUploadedToBlob":
      return <span style={{ color: "goldenrod" }}>-</span>;
    case "Indexed":
      return <span style={{ color: "green" }}>+</span>;
    default:
      return <span style={{ color: "red" }}>x</span>;
  }
};

export const TrackerVisualisation: React.FC<Props> = ({ pipelineState }) => {
  if (!pipelineState.haveData) {
    return null;
  }

  if (!pipelineState.data.documents.length) {
    return (
      <div style={{ textAlign: "center", fontFamily: "monospace" }}>
        This case has no documents
      </div>
    );
  }

  return (
    <div style={{ textAlign: "center", fontFamily: "monospace" }}>
      {pipelineState.data.documents.map((doc) => {
        return <span key={doc.documentId}>{renderDoc(doc)}</span>;
      })}
    </div>
  );
};
