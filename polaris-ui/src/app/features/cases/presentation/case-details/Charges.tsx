import { CaseDetails } from "../../domain/gateway/CaseDetails";
import { getFormattedCustodyTimeData } from "../case-details/utils/chargesUtil";
import classes from "./index.module.scss";

export const Charges: React.FC<{ caseDetails: CaseDetails }> = ({
  caseDetails,
}) => {
  const { custodyExpiryDays, custodyExpiryDate } = getFormattedCustodyTimeData(
    caseDetails.defendants?.[0]?.custodyTimeLimit
  );
  return (
    <div className={classes.charges} data-testid="div-charges">
      <h2 className="govuk-heading-s" data-testid="charges-title">
        {caseDetails.isCaseCharged ? "Charges" : "Proposed Charges"}
      </h2>
      <ul>
        <li>{caseDetails.headlineCharge.charge}</li>
      </ul>
      <div className={classes.chargesCustodyTime}>
        <span>Custody time limit: {custodyExpiryDays}</span>
        <span>Custody end: {custodyExpiryDate}</span>
      </div>
    </div>
  );
};
