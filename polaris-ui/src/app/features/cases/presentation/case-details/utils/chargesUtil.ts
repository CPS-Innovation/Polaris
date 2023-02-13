import { CustodyTimeLimit } from "../../../domain/CaseDetails";
import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../common/utils/dates";
export const getFormattedCustodyTimeData = (
  custodyTimeLimit: CustodyTimeLimit
) => {
  if (!custodyTimeLimit) {
    return {
      custodyExpiryDays: "N/A",
      custodyExpiryDate: "N/A",
    };
  }
  const { expiryDate, expiryDays, expiryIndicator } = custodyTimeLimit;

  switch (expiryIndicator) {
    case "Active":
      return {
        custodyExpiryDays: `${expiryDays} ${expiryDays > 1 ? "Days" : "Day"}`,
        custodyExpiryDate: formatDate(
          expiryDate,
          CommonDateTimeFormats.ShortDateTextMonth
        ),
      };

    case "Expired":
      return {
        custodyExpiryDays: "Expired",
        custodyExpiryDate: formatDate(
          expiryDate,
          CommonDateTimeFormats.ShortDateTextMonth
        ),
      };
    default:
      return {
        custodyExpiryDays: "N/A",
        custodyExpiryDate: "N/A",
      };
  }
};
