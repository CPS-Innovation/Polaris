import {
  CommonDateTimeFormats,
  formatDate,
  getAgeFromIsoDate,
} from "../../../../common/utils/dates";
import { Tag } from "../../../../common/presentation/components";
import {
  CaseDetails,
  Defendant,
  DefendantDetails,
} from "../../domain/gateway/CaseDetails";
import { LinkButton } from "../../../../../app/common/presentation/components/LinkButton";
import classes from "./index.module.scss";

export const KeyDetails: React.FC<{
  caseDetails: CaseDetails;
  isMultipleDefendantsOrCharges: boolean;
  handleOpenPdf: () => void;
  dacDocumentId: string;
}> = ({
  caseDetails,
  isMultipleDefendantsOrCharges,
  handleOpenPdf,
  dacDocumentId,
}) => {
  const getOrderedDefendantsList = (caseDetails: CaseDetails) => {
    const { defendants } = caseDetails;
    defendants.sort(
      (a, b) => a.defendantDetails.listOrder - b.defendantDetails.listOrder
    );
    return defendants;
  };

  const getDefendantName = (defendantDetail: DefendantDetails | null) => {
    if (!defendantDetail) return "";
    if (defendantDetail.type === "Organisation") {
      return defendantDetail.organisationName;
    }
    return `${defendantDetail.surname}, ${defendantDetail.firstNames}`;
  };

  const getDefendantNameText = (
    isMultipleDefendantsOrCharges: boolean,
    defendantsList: Defendant[] = []
  ) => {
    if (!isMultipleDefendantsOrCharges) {
      return getDefendantName(caseDetails?.leadDefendantDetails);
    }
    return defendantsList.reduce((acc, item) => {
      const { defendantDetails } = item;
      if (!acc) return `${getDefendantName(defendantDetails)}`;
      return `${acc}; ${getDefendantName(defendantDetails)}`;
    }, "");
  };

  const getDOBText = () => {
    if (
      isMultipleDefendantsOrCharges ||
      !caseDetails ||
      !caseDetails.leadDefendantDetails ||
      caseDetails.leadDefendantDetails.type === "Organisation"
    ) {
      return "";
    }
    return (
      <h2
        className={`govuk-heading-s ${classes.defendantDOB}`}
        data-testid="txt-defendant-DOB"
      >
        DOB:{" "}
        <span className={classes.dobValue}>
          {formatDate(
            caseDetails.leadDefendantDetails.dob,
            CommonDateTimeFormats.ShortDateTextMonth
          )}
        </span>
        , Age:{" "}
        <span className={classes.ageValue}>
          {getAgeFromIsoDate(caseDetails.leadDefendantDetails.dob)}
        </span>
      </h2>
    );
  };

  const isYouthOffender = () => {
    if (
      !isMultipleDefendantsOrCharges &&
      caseDetails?.leadDefendantDetails?.youth
    )
      return true;
    return false;
  };

  const defendantsList = getOrderedDefendantsList(caseDetails);

  return (
    <div className={`${classes.keyDetails}`} data-testid="key-details">
      {
        <>
          {getDefendantNameText(
            isMultipleDefendantsOrCharges,
            defendantsList
          ) && (
            <h1
              className={`govuk-heading-m ${classes.defendantName}`}
              data-testid="defendant-name"
            >
              {getDefendantNameText(
                isMultipleDefendantsOrCharges,
                defendantsList
              )}
            </h1>
          )}
          <h2
            className={`govuk-heading-s ${classes.uniqueReferenceNumber}`}
            data-testid="txt-case-urn"
          >
            {caseDetails.uniqueReferenceNumber}
          </h2>
          {getDOBText() && getDOBText()}
          {isYouthOffender() && (
            <Tag className="govuk-tag--blue"> Youth offender</Tag>
          )}
          {isMultipleDefendantsOrCharges && dacDocumentId && (
            <LinkButton
              dataTestId="link-defendant-details"
              className={classes.defendantDetailsLink}
              onClick={handleOpenPdf}
            >
              {`View ${defendantsList.length} ${
                defendantsList.length > 1 ? "defendants" : "defendant"
              } and charges`}
            </LinkButton>
          )}
        </>
      }
    </div>
  );
};
