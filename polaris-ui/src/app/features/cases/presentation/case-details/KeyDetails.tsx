import {
  CommonDateTimeFormats,
  formatDate,
  getAgeFromIsoDate,
} from "../../../../common/utils/dates";
import { CaseDetails } from "../../domain/CaseDetails";
import classes from "./index.module.scss";

export const KeyDetails: React.FC<{ caseDetails: CaseDetails }> = ({
  caseDetails,
}) => {
  return (
    <div>
      <h1
        className={`govuk-heading-m ${classes.uniqueReferenceNumber}`}
        data-testid="txt-case-urn"
      >
        {caseDetails.uniqueReferenceNumber}
      </h1>

      <div className={`govuk-heading-s`} data-testid="txt-defendant-name">
        <div>
          {caseDetails.leadDefendantDetails.surname},{" "}
          {caseDetails.leadDefendantDetails.firstNames}
        </div>
        <div className={`${classes.namesub}`}>
          DOB:
          {formatDate(
            caseDetails.leadDefendantDetails.dob,
            CommonDateTimeFormats.ShortDateTextMonth
          )}
          . Age: {getAgeFromIsoDate(caseDetails.leadDefendantDetails.dob)}
        </div>
      </div>
    </div>
  );
};
