import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { TrackerVisualisation } from "./TrackerVisualisation";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
};

export const PdfTabsEmpty: React.FC<Props> = ({ pipelineState }) => (
  <div>
    <h2
      className="govuk-heading-m"
      style={{ margin: "220px 0 0 0", textAlign: "center" }}
    >
      View your documents
    </h2>
    <p style={{ margin: "margin:10px 0 0 0", textAlign: "center" }}>
      Search or choose a file to start reviewing documents
    </p>

    <TrackerVisualisation pipelineState={pipelineState} />
  </div>
);
