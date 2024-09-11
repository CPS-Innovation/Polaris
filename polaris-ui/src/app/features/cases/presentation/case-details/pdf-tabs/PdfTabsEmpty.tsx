import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { TrackerSummary } from "./TrackerSummary";
import classes from "./PdfTabsEmpty.module.scss";
import { TaggedContext } from "../../../../../inbound-handover/context";

type Props = {
  pipelineState: CaseDetailsState["pipelineState"];
  isMultipleDefendantsOrCharges: boolean;
  context: TaggedContext | undefined;
};

export const PdfTabsEmpty: React.FC<Props> = ({
  pipelineState,
  isMultipleDefendantsOrCharges,
  context,
}) => (
  <div className={`${classes.pdfTabsEmpty}`}>
    <h2 className={`govuk-heading-m ${classes.heading}`}>
      View your documents
    </h2>
    <p className={`${classes.content}`}>
      Search or choose a file to start reviewing documents
    </p>

    <TrackerSummary
      pipelineState={pipelineState}
      isMultipleDefendantsOrCharges={isMultipleDefendantsOrCharges}
    />

    <TemporaryContextFeedback context={context} />
  </div>
);

// todo: remove this once we have implemented some context-specific functionality
//  At the moment this exist to allow a cypress e2e test to assert as successful
//  test - that should be changed and new test logic that assert on real functionality
//  should replace it.
const TemporaryContextFeedback: React.FC<{
  context: TaggedContext | undefined;
}> = ({ context }) => {
  return (
    <div aria-hidden="true" style={{ color: "#FFFFFF" }}>
      {JSON.stringify(context)}
    </div>
  );
};
