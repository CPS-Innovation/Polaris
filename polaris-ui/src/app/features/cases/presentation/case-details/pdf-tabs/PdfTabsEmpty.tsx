import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { TrackerSummary } from "./TrackerSummary";
import classes from "./PdfTabsEmpty.module.scss";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
};

export const PdfTabsEmpty: React.FC<Props> = ({ pipelineState }) => (
  <div className={`${classes.pdfTabsEmpty}`}>
    <h2 className={`govuk-heading-m ${classes.heading}`}>
      View your documents
    </h2>
    <p className={`${classes.content}`}>
      Search or choose a file to start reviewing documents
    </p>

    <TrackerSummary pipelineState={pipelineState} />
  </div>
);
