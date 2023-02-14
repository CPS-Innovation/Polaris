import {
  CommonDateTimeFormats,
  formatDate,
  getAgeFromIsoDate,
} from "../../../../common/utils/dates";
import { CaseDetails } from "../../domain/CaseDetails";
import { LinkButton } from "../../../../../app/common/presentation/components/LinkButton";
import classes from "./index.module.scss";

export const KeyDetails: React.FC<{
  caseDetails: CaseDetails;
  isMultipleDefendantsOrCharges: boolean;
}> = ({ caseDetails, isMultipleDefendantsOrCharges }) => {
  const getOrderedDefendantsList = (caseDetails: CaseDetails) => {
    const { defendants } = caseDetails;
    defendants.sort(
      (a, b) => a.defendantDetails.listOrder - b.defendantDetails.listOrder
    );
    return defendants;
  };

  const defendantsList = getOrderedDefendantsList(caseDetails);

  return (
    <div>
      <h1
        className={`govuk-heading-m ${classes.uniqueReferenceNumber}`}
        data-testid="txt-case-urn"
      >
        {caseDetails.uniqueReferenceNumber}
      </h1>

      {isMultipleDefendantsOrCharges && (
        <>
          <ul
            className={classes.defendantsList}
            data-testid="list-defendant-names"
          >
            {defendantsList.map(({ defendantDetails }) => (
              <li key={defendantDetails.id}>
                {defendantDetails.surname}, {defendantDetails.firstNames}
              </li>
            ))}
          </ul>
          <LinkButton
            data-testid="link-defendant-details"
            className={classes.defendantDetailsLink}
            onClick={() => {}}
          >
            {`View ${defendantsList.length} ${
              defendantsList.length > 1 ? "defendants" : "defendant"
            } and charges`}
          </LinkButton>
        </>
      )}
      {!isMultipleDefendantsOrCharges && (
        <div
          className={classes.defendantDetails}
          data-testid="defendant-details"
        >
          <span className={`govuk-heading-s ${classes.defendantName}`}>
            {defendantsList[0].defendantDetails.surname},{" "}
            {defendantsList[0].defendantDetails.firstNames}
          </span>
          <span className={`${classes.defendantDOB}`}>
            DOB:{" "}
            {formatDate(
              defendantsList[0].defendantDetails.dob,
              CommonDateTimeFormats.ShortDateTextMonth
            )}
            . Age: {getAgeFromIsoDate(defendantsList[0].defendantDetails.dob)}
          </span>
        </div>
      )}
    </div>
  );
};
