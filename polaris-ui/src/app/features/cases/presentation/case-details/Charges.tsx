import { CaseDetails } from "../../domain/CaseDetails";
import classes from "./index.module.scss";

export const Charges: React.FC<{ caseDetails: CaseDetails }> = ({
  caseDetails,
}) => {
  return (
    <div className={classes.charges}>
      <h2 className="govuk-heading-s">
        {caseDetails.isCaseCharged ? "Charges" : "Proposed Charges"}
      </h2>
      <ul>
        <li>{caseDetails.headlineCharge.charge}</li>
      </ul>
    </div>
  );
};
